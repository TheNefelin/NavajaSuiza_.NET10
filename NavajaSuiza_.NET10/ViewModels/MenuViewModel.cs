using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class MenuViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDeviceStatusService _deviceStatusService;

    [ObservableProperty]
    public partial string AvailableStorage { get; set; } = "0 GB";

    [ObservableProperty]
    public partial string BatteryLevel { get; set; } = "0%";

    [ObservableProperty]
    public partial bool IsDevelopment { get; set; }

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
    private async Task NavigateToFraming()
    {
        var page = _serviceProvider.GetRequiredService<FramingPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToTuner()
    {
        var page = _serviceProvider.GetRequiredService<TunerPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    
    [RelayCommand]
    private async Task NavigateToMetronome()
    {
        var page = _serviceProvider.GetRequiredService<MetronomePage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToCompass()
    {
        var page = _serviceProvider.GetRequiredService<CompassPage>();
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
        await Shell.Current.GoToAsync("//AboutPage");
    }

    [RelayCommand]
    private async Task NavigateToTest()
    {
        var page = _serviceProvider.GetRequiredService<TestingPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }
}
