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

        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
