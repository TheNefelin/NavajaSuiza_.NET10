using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Core.ViewModels;

public partial class MenuViewModel : BaseViewModel
{
    private const string NotAvailable = "N/A";
    private const int BatteryDangerMax = 30;
    private const int BatteryWarningMax = 59;
    private const long StorageDangerBytes = 2L * 1024 * 1024 * 1024;
    private const long StorageWarningBytes = 5L * 1024 * 1024 * 1024;

    private readonly ILogger<MenuViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IDeviceStatusService _deviceStatusService;
    private readonly IFilePickerService _filePickerService;
    private readonly ILanguageService _languageService;
    private readonly IDocumentPdfConverter _documentPdfConverter;

    [ObservableProperty]
    public partial string AvailableStorage { get; set; } = NotAvailable;

    [ObservableProperty]
    public partial StatusLevel StorageStatus { get; set; } = StatusLevel.Unknown;

    [ObservableProperty]
    public partial string BatteryLevel { get; set; } = NotAvailable;

    [ObservableProperty]
    public partial StatusLevel BatteryStatus { get; set; } = StatusLevel.Unknown;

    [ObservableProperty]
    public partial bool IsDevelopment { get; set; }

    [ObservableProperty]
    public partial bool IsConverting { get; set; }

    public MenuViewModel(
        ILogger<MenuViewModel> logger,
        INavigationService navigationService,
        IDeviceStatusService deviceStatusService,
        IFilePickerService filePickerService,
        ILanguageService languageService,
        IDocumentPdfConverter documentPdfConverter)
    {
        _logger = logger;
        _navigationService = navigationService;
        _deviceStatusService = deviceStatusService;
        _filePickerService = filePickerService;
        _languageService = languageService;
        _documentPdfConverter = documentPdfConverter;
    }

    public void OnPageAppearing()
    {
        UpdateBattery();
        UpdateStorage();
    }

    private void UpdateBattery()
    {
        int capacity = _deviceStatusService.GetBatteryCapacity();

        if (capacity is < 0 or > 100)
        {
            BatteryLevel = NotAvailable;
            BatteryStatus = StatusLevel.Unknown;
            return;
        }

        BatteryLevel = $"{capacity}%";
        BatteryStatus = capacity switch
        {
            <= BatteryDangerMax => StatusLevel.Danger,
            <= BatteryWarningMax => StatusLevel.Warning,
            _ => StatusLevel.Ok
        };
    }

    private void UpdateStorage()
    {
        long available = _deviceStatusService.GetAvailableStorageBytes();

        if (available < 0)
        {
            AvailableStorage = NotAvailable;
            StorageStatus = StatusLevel.Unknown;
            return;
        }

        AvailableStorage = FormatBytes(available);

        // El almacenamiento se evalua por espacio absoluto, no por porcentaje:
        // 20% libre puede ser correcto en un disco de 128 GB y critico en uno de 16 GB.
        StorageStatus = available switch
        {
            < StorageDangerBytes => StatusLevel.Danger,
            < StorageWarningBytes => StatusLevel.Warning,
            _ => StatusLevel.Ok
        };
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    [RelayCommand]
    private async Task NavigateToFlashlight() => await _navigationService.PushAsync("FlashlightPage");

    [RelayCommand]
    private async Task NavigateToPdfReader()
    {
        var path = await _filePickerService.PickDocumentAsync(
            _languageService.GetString("PdfReaderPickerTitleText"));
        if (string.IsNullOrEmpty(path))
            return;

        var documentType = DocumentTypeDetector.Detect(path);

        PdfReaderPayload payload;
        switch (documentType)
        {
            case DocumentType.Pdf:
                payload = new PdfReaderPayload
                {
                    Path = path,
                    FileName = Path.GetFileName(path)
                };
                break;

            case DocumentType.Docx:
            case DocumentType.Xlsx:
            case DocumentType.Doc:
            case DocumentType.Xls:
            case DocumentType.Csv:
                IsConverting = true;
                try
                {
                    var stream = await _documentPdfConverter.ConvertToPdfAsync(documentType, path);
                    payload = new PdfReaderPayload
                    {
                        Stream = stream,
                        FileName = Path.GetFileName(path)
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al convertir {Type} a PDF: {Path}", documentType, path);
                    await _navigationService.DisplayAlertAsync(
                        string.Empty,
                        _languageService.GetString("PdfReaderOpenErrorText"),
                        _languageService.GetString("CommonOkText"));
                    return;
                }
                finally
                {
                    IsConverting = false;
                }

                break;

            case DocumentType.Text:
                await _navigationService.PushAsync("TextReaderPage", path);
                return;

            default:
                _logger.LogWarning("Formato no soportado: {Path}", path);
                return;
        }

        await _navigationService.PushAsync("PdfReaderPage", payload);
    }

    [RelayCommand]
    private async Task NavigateToManual() => await _navigationService.PushAsync("ManualPage");

    [RelayCommand]
    private async Task NavigateToNotes() => await _navigationService.PushAsync("NotesPage");

    [RelayCommand]
    private async Task NavigateToBoard() => await _navigationService.PushAsync("BoardPage");

    [RelayCommand]
    private async Task NavigateToTuner() => await _navigationService.PushAsync("TunerPage");

    [RelayCommand]
    private async Task NavigateToMetronome() => await _navigationService.PushAsync("MetronomePage");

    [RelayCommand]
    private async Task NavigateToStopwatch() => await _navigationService.PushAsync("StopwatchPage");

    [RelayCommand]
    private async Task NavigateToCompass() => await _navigationService.PushAsync("CompassPage");
}