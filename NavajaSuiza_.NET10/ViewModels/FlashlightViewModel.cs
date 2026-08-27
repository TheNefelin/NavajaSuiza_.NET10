using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    public partial bool IsFlashOn { get; set; }

    [ObservableProperty]
    public partial bool IsScreenOn { get; set; }

    [ObservableProperty]
    public partial bool IsLightOn { get; set; }

    public FlashlightViewModel(
        INavigationService navigationService,
        ILanguageService languageService)
    {
        _navigationService = navigationService;
        _languageService = languageService;
        _languageService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsLightOn));
    }

    public override void Cleanup()
    {
        _languageService.LanguageChanged -= OnLanguageChanged;
        base.Cleanup();
    }

    [RelayCommand]
    private async Task ClickFlash()
    {
        IsFlashOn = !IsFlashOn;

        try
        {
            if (IsFlashOn)
            {
                await Flashlight.Default.TurnOnAsync();
            }
            else
            {
                await Flashlight.Default.TurnOffAsync();
            }
        }
        catch (Exception)
        {
            IsFlashOn = !IsFlashOn;
        }
    }

    [RelayCommand]
    private async Task ClickScreen()
    {
        IsScreenOn = !IsScreenOn;

        try
        {
            if (IsScreenOn)
            {
                await _navigationService.PushAsync("ScreenLightPage");
                IsScreenOn = false;
            }
            else
            {
                DeviceDisplay.Current.KeepScreenOn = false;
            }
        }
        catch (Exception)
        {
            IsScreenOn = !IsScreenOn;
        }
    }
}
