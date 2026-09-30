#if ANDROID
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
#endif
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ThemeService : IThemeService
{
    public event EventHandler? ThemeChanged;

    public bool IsDarkTheme { get; private set; } = true;

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
            SaveThemePreference(true);
            return true;
        }
    }

    public void SaveThemePreference(bool isDarkMode)
    {
        Preferences.Set("ThemeMode", isDarkMode ? 1 : 0);
        ApplyTheme(isDarkMode);
    }

    private void ApplyTheme(bool isDarkMode)
    {
        IsDarkTheme = isDarkMode;
        Application.Current!.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;

        UpdateStatusBarColors(isDarkMode);

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateStatusBarColors(bool isDarkMode)
    {
#if ANDROID
        try
        {
            var resourceKey = isDarkMode ? "MyBackgroundMenuDark" : "MyBackgroundMenuLight";

            if (ResolveThemeColor(resourceKey) is not { } color)
            {
                return;
            }

            var window = Platform.CurrentActivity?.Window;

            if (window == null)
            {
                return;
            }

            var androidColor = color.ToPlatform();

#pragma warning disable CA1422 // Deprecated in Android 35; still functional on target API levels
            window.SetStatusBarColor(androidColor);
            window.SetNavigationBarColor(androidColor);
#pragma warning restore CA1422

            var controller = WindowCompat.GetInsetsController(window, window.DecorView);

            if (controller is null)
            {
                return;
            }

            controller.AppearanceLightStatusBars = !isDarkMode;
            controller.AppearanceLightNavigationBars = !isDarkMode;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"System bar appearance failed: {exception.Message}");
        }
#endif
    }

    private static Microsoft.Maui.Graphics.Color? ResolveThemeColor(string key)
    {
        return Application.Current?.Resources?.TryGetValue(key, out var value) == true
            && value is Microsoft.Maui.Graphics.Color resolved
                ? resolved
                : null;
    }
}
