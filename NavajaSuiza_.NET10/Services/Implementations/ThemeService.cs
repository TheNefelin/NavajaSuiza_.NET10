using NavajaSuiza.Core.Interfaces;

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
            var color = Android.Graphics.Color.ParseColor("#243042");
            var activity = Platform.CurrentActivity;

            if (activity?.Window != null)
            {
#pragma warning disable CA1422 // Deprecated in Android 35; still functional on target API levels
                activity.Window.SetStatusBarColor(color);
                activity.Window.SetNavigationBarColor(color);
#pragma warning restore CA1422
            }
        }
        catch (Exception)
        {
        }
#endif

    }
}
