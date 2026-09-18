using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly IAppInfoService _appInfoService;
    private readonly ILauncherService _launcherService;

    [ObservableProperty]
    public partial bool IsDarkMode { get; set; }

    public string VersionText => $"v{_appInfoService.Version} (build {_appInfoService.Build})";

    public bool HasDonationUrl => !string.IsNullOrWhiteSpace(AppConstants.About.DonationUrl);

    public AboutViewModel(
        IThemeService themeService,
        IAppInfoService appInfoService,
        ILauncherService launcherService)
    {
        _themeService = themeService;
        _appInfoService = appInfoService;
        _launcherService = launcherService;

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

    [RelayCommand]
    private async Task OpenDonationAsync()
    {
        if (HasDonationUrl)
        {
            await _launcherService.OpenAsync(AppConstants.About.DonationUrl);
        }
    }

    [RelayCommand]
    private async Task OpenWebsiteAsync()
    {
        await _launcherService.OpenAsync(AppConstants.About.WebsiteUrl);
    }
}