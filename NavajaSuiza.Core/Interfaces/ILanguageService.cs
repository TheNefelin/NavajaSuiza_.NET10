namespace NavajaSuiza.Core.Interfaces;

public interface ILanguageService
{
    event EventHandler LanguageChanged;

    void InitializeLanguage();

    string GetCurrentLanguage();

    void SetLanguage(string languageCode);

    string ExtractLanguageCode(string selection);

    void SaveLanguage(string languageCode);

    string GetString(string resourceKey);
}
