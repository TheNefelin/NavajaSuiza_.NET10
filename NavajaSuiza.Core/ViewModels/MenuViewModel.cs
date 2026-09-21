using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Core.ViewModels;

public partial class MenuViewModel : BaseViewModel
{
    private readonly ILogger<MenuViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IDeviceStatusService _deviceStatusService;
    private readonly IFilePickerService _filePickerService;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    public partial string AvailableStorage { get; set; } = "0 GB";

    [ObservableProperty]
    public partial string BatteryLevel { get; set; } = "0%";

    [ObservableProperty]
    public partial bool IsDevelopment { get; set; }

    public MenuViewModel(
        ILogger<MenuViewModel> logger,
        INavigationService navigationService,
        IDeviceStatusService deviceStatusService,
        IFilePickerService filePickerService,
        ILanguageService languageService)
    {
        _logger = logger;
        _navigationService = navigationService;
        _deviceStatusService = deviceStatusService;
        _filePickerService = filePickerService;
        _languageService = languageService;
    }

    public void OnPageAppearing()
    {
        BatteryLevel = _deviceStatusService.GetBatteryLevel();
        AvailableStorage = _deviceStatusService.GetAvailableStorage();
    }

    [RelayCommand]
    private async Task NavigateToFlashlight() => await _navigationService.PushAsync("FlashlightPage");

    [RelayCommand]
    private async Task NavigateToPdfReader()
    {
        var path = await _filePickerService.PickPdfAsync(
            _languageService.GetString("PdfReaderPickerTitleText"));
        if (string.IsNullOrEmpty(path))
            return;

        await _navigationService.PushAsync("PdfReaderPage", path);
    }

    [RelayCommand]
    private async Task NavigateToManual() => await _navigationService.PushAsync("ManualPage");

    [RelayCommand]
    private async Task NavigateToNotes() => await _navigationService.PushAsync("NotesPage");

    [RelayCommand]
    private async Task NavigateToPizarra() => await _navigationService.PushAsync("PizarraPage");

    [RelayCommand]
    private async Task NavigateToDocumentReader()
    {
        var path = await _filePickerService.PickDocumentAsync(
            _languageService.GetString("DocumentReaderPickerTitleText"));
        if (string.IsNullOrEmpty(path))
            return;

        await _navigationService.PushAsync("DocumentReaderPage", path);
    }
}