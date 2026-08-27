using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class MenuViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IDeviceStatusService _deviceStatusService;

    [ObservableProperty]
    public partial string AvailableStorage { get; set; } = "0 GB";

    [ObservableProperty]
    public partial string BatteryLevel { get; set; } = "0%";

    [ObservableProperty]
    public partial bool IsDevelopment { get; set; }

    public MenuViewModel(
        INavigationService navigationService,
        IDeviceStatusService deviceStatusService)
    {
        _navigationService = navigationService;
        _deviceStatusService = deviceStatusService;
    }

    public void OnPageAppearing()
    {
        BatteryLevel = _deviceStatusService.GetBatteryLevel();
        AvailableStorage = _deviceStatusService.GetAvailableStorage();
    }

    [RelayCommand]
    private async Task NavigateToFlashlight() => await _navigationService.PushAsync("FlashlightPage");

    [RelayCommand]
    private async Task NavigateToFraming() => await _navigationService.PushAsync("FramingPage");

    [RelayCommand]
    private async Task NavigateToTuner() => await _navigationService.PushAsync("TunerPage");

    [RelayCommand]
    private async Task NavigateToMetronome() => await _navigationService.PushAsync("MetronomePage");

    [RelayCommand]
    private async Task NavigateToCompass() => await _navigationService.PushAsync("CompassPage");

    [RelayCommand]
    private async Task NavigateToManual() => await _navigationService.PushAsync("ManualPage");

    [RelayCommand]
    private async Task NavigateToAbout() => await _navigationService.GoToAsync("//AboutPage");

    [RelayCommand]
    private async Task NavigateToTest() => await _navigationService.PushAsync("TestingPage");
}
