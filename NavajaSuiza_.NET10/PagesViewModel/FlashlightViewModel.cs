using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

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

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsLightOn));
    }

    [RelayCommand]
    private async void ClickFlash()
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
        catch (Exception ex)
        {
            IsFlashOn = !IsFlashOn; // Revierte el cambio si falla
            //await Shell.Current.DisplayAlertAsync(
            //    "Error",
            //    "No se pudo acceder a la linterna. Verifica los permisos.",
            //    "OK");
        }
    }
  
    [RelayCommand]
    private async void ClickScreen()
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
        catch (Exception ex)
        {
            _ = Shell.Current.DisplayAlertAsync(
                "Error",
                ex.Message,
                "OK");
        }
    }

    //[RelayCommand]
    //private async void ClickLight()
    //{
    //    IsLightOn = !IsLightOn;
    //}
}
