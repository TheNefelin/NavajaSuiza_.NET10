namespace NavajaSuiza.Core.Interfaces;

public interface IThemeService
{
    /// <summary>
    /// Se dispara despues de aplicar el tema. Sirve para que superficies que no
    /// participan de AppThemeBinding, como el HTML del WebView de la guia,
    /// puedan re-renderizarse.
    /// </summary>
    event EventHandler? ThemeChanged;

    bool IsDarkTheme { get; }

    bool ApplySavedTheme();

    void SaveThemePreference(bool isDarkMode);
}