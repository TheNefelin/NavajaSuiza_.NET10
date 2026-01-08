using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class MenuViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDeviceStatusService _deviceStatusService;

    [ObservableProperty]
    private string _availableStorage = "0 GB";

    [ObservableProperty]
    private string _batteryLevel = "0%";

    public MenuViewModel(
        IServiceProvider serviceProvider,
        IDeviceStatusService deviceStatusService)
    {
        _serviceProvider = serviceProvider;
        _deviceStatusService = deviceStatusService;
    }

    public void OnPageAppearing()
    {
        BatteryLevel = _deviceStatusService.GetBatteryLevel();
        AvailableStorage = _deviceStatusService.GetAvailableStorage();
    }

    [RelayCommand]
    private async Task NavigateToFlashlight()
    {
        var page = _serviceProvider.GetRequiredService<FlashlightPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToTuner()
    {
        var page = _serviceProvider.GetRequiredService<TunerPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToManual()
    {
        var page = _serviceProvider.GetRequiredService<ManualPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToAbout()
    {
        var page = _serviceProvider.GetRequiredService<AboutPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToTest()
    {
        var page = _serviceProvider.GetRequiredService<TestingPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }
}
