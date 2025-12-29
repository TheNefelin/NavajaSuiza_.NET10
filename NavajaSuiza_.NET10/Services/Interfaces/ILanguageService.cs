namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface ILanguageService
{
    void InitializeLanguage();
    /// <summary>
    /// Get the current application language
    /// <summary>
    string GetCurrentLanguage();
    /// <summary>
    /// Change the application language
    /// </summary>
    void SetLanguage(string languageCode);

    /// <summary>
    /// Extract language code from user selection
    /// </summary>
    string ExtractLanguageCode(string selection);

    void SaveLanguage(string languageCode);
}
