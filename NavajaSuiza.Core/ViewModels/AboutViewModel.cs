using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    public partial bool IsDarkMode { get; set; }

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
