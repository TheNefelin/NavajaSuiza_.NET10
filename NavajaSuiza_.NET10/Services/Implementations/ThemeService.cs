using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ThemeService : IThemeService
{
    public bool ApplySavedTheme()
    {
        if (Preferences.ContainsKey("ThemeMode"))
        {
            var savedTheme = Preferences.Get("ThemeMode", 1);
            var isDarkMode = savedTheme == 1;

            ApplyTheme(isDarkMode);
            return isDarkMode;
        }
        else
        {
            var isDarkMode = true;
            ApplyTheme(isDarkMode);
            return isDarkMode;
        }
    }

    public void SaveThemePreference(bool isDarkMode)
    {
        Preferences.Set("ThemeMode", isDarkMode ? 1 : 0);
        ApplyTheme(isDarkMode);
    }

    private void ApplyTheme(bool isDarkMode)
    {
        Application.Current!.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

        UpdateStatusBarColors(isDarkMode);
    }

    private void UpdateStatusBarColors(bool isDarkMode)
    {
#if ANDROID
        try
        {
            var window = (Application.Current as App)?.MainPage?.Window?.Handler?.PlatformView as Android.App.Activity;

            if (window != null)
            {
                if (isDarkMode)
                {
                    var statusBarColor = Android.Graphics.Color.ParseColor("#243042");
                    window.Window?.SetStatusBarColor(statusBarColor);
                    window.Window?.SetNavigationBarColor(statusBarColor);
                }
                else
                {
                    var statusBarColor = Android.Graphics.Color.ParseColor("#F7F5F0");
                    window.Window?.SetStatusBarColor(statusBarColor);
                    window.Window?.SetNavigationBarColor(statusBarColor);
                }

                //_logger.LogInformation("[ThemeService] - Status bar colors updated: {Mode}", isDarkMode ? "Dark" : "Light");
            }
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "[ThemeService] - Error updating status bar colors");
        }
#endif
    }
}
