using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.ComponentModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private bool _isFlashOn = false;

    [ObservableProperty]
    private bool _isScreenOn = false;

    [ObservableProperty]
    private bool _isLightOn = false;

    public FlashlightViewModel(
        ILanguageService languageService)
    {
        _languageService = languageService;
        // Suscríbete a cambios de idioma
        _languageService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsLightOn));
    }

    [RelayCommand]
    private async Task ClickFlashAsync() => IsFlashOn = !IsFlashOn;
  
    [RelayCommand]
    private async void ClickScreen() => IsScreenOn = !IsScreenOn;

    [RelayCommand]
    private async void ClickLight()
    {
        IsLightOn = !IsLightOn;
        // Fuerza re-evaluación del converter cuando cambia
        //OnPropertyChanged(nameof(IsLightOn));
    }
}
