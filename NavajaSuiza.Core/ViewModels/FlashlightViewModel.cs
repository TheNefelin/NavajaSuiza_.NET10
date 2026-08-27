using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly IFlashlightService _flashlightService;
    private readonly IDeviceDisplayService _deviceDisplayService;
    private readonly IFlashlightStateService _stateService;

    public bool IsFlashOn
    {
        get => _stateService.IsFlashOn;
        set
        {
            if (_stateService.IsFlashOn != value)
            {
                _stateService.IsFlashOn = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    public partial bool IsScreenOn { get; set; }

    [ObservableProperty]
    public partial bool IsLightOn { get; set; }

    public FlashlightViewModel(
        INavigationService navigationService,
        ILanguageService languageService,
        IFlashlightService flashlightService,
        IDeviceDisplayService deviceDisplayService,
        IFlashlightStateService stateService)
    {
        _navigationService = navigationService;
        _languageService = languageService;
        _flashlightService = flashlightService;
        _deviceDisplayService = deviceDisplayService;
        _stateService = stateService;
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
                await _flashlightService.TurnOnAsync();
            }
            else
            {
                await _flashlightService.TurnOffAsync();
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
                _deviceDisplayService.KeepScreenOn = false;
            }
        }
        catch (Exception)
        {
            IsScreenOn = !IsScreenOn;
        }
    }
}
