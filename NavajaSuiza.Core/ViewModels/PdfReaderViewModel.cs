using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class PdfReaderViewModel : BaseViewModel
{
    private readonly ILogger<PdfReaderViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly IFilePickerService _filePickerService;

    [ObservableProperty]
    public partial Stream? PdfDocumentStream { get; set; }

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HintText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFileLoaded { get; set; }

    public PdfReaderViewModel(
        ILogger<PdfReaderViewModel> logger,
        ILanguageService languageService,
        IFilePickerService filePickerService)
    {
        _logger = logger;
        _languageService = languageService;
        _filePickerService = filePickerService;
        HintText = _languageService.GetString("PdfReaderEmptyHintText");
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var path = await _filePickerService.PickPdfAsync(_languageService.GetString("PdfReaderPickerTitleText"));
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            Unload();
            PdfDocumentStream = stream;
            FileName = Path.GetFileName(path);
            HintText = string.Empty;
            IsFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir el PDF {Path}", path);
            Unload();
            HintText = _languageService.GetString("PdfReaderOpenErrorText");
        }
    }

    public void Unload()
    {
        PdfDocumentStream?.Dispose();
        PdfDocumentStream = null;
        FileName = string.Empty;
        IsFileLoaded = false;
        HintText = _languageService.GetString("PdfReaderEmptyHintText");
    }
}