namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface ILanguageService
{
    /// <summary>
    /// Event triggered when the application language changes
    /// <summary> 
    event EventHandler LanguageChanged;

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

    /// <summary>
    /// Save the selected language code to persistent storage
    /// <summary>
    void SaveLanguage(string languageCode);
}
