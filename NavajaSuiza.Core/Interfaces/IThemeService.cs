namespace NavajaSuiza.Core.Interfaces;

public interface IThemeService
{
    bool ApplySavedTheme();
    void SaveThemePreference(bool isDarkMode);
}
