using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class AboutViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private bool _isDarkMode;

    public AboutViewModel(
        IThemeService themeService)
    {
        _themeService = themeService;

        LoadThemePreference();
    }

    private void LoadThemePreference()
    {
        IsDarkMode = _themeService.ApplySavedTheme();
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _themeService.SaveThemePreference(value);
    }
}
