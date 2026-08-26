using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private bool _isFlashOn = false;

    [ObservableProperty]
    private bool _isScreenOn = false;

    [ObservableProperty]
    private bool _isLightOn = false;

    public FlashlightViewModel(
        IServiceProvider serviceProvider,
        ILanguageService languageService)
    {
        _serviceProvider = serviceProvider;
        _languageService = languageService;
        // Suscríbete a cambios de idioma
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
            IsFlashOn = !IsFlashOn; // Revierte el cambio si falla
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
                var screenLightPage = _serviceProvider.GetRequiredService<ScreenLightPage>();
                await Shell.Current.Navigation.PushAsync(screenLightPage);
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
