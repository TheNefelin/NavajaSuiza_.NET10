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
            var color = Android.Graphics.Color.ParseColor("#243042");
            var activity = (Application.Current?.MainPage?.Window?.Handler?.PlatformView as Android.App.Activity);

            if (activity != null)
            {
                activity.Window?.SetStatusBarColor(color);
                activity.Window?.SetNavigationBarColor(color);
            }
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "[ThemeService] - Error updating status bar");
        }
#endif

//#if ANDROID
//        try
//        {
//            var activity = (Application.Current?.MainPage?.Window?.Handler?.PlatformView as Android.App.Activity);
//            if (activity != null)
//            {
//                if (isDarkMode)
//                {
//                    // Dark mode
//                    activity.Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#243042"));
//                    activity.Window?.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#243042"));
//                }
//                else
//                {
//                    // Light mode
//                    activity.Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#F7F5F0"));
//                    activity.Window?.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#F7F5F0"));
//                }

//                //activity.Recreate();
//            }
//        }
//        catch (Exception ex)
//        {
//            //_logger.LogError(ex, "[ThemeService] - Error updating status bar");
//        }
//#endif
    }
}
