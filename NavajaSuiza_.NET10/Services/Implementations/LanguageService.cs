using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Globalization;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class LanguageService : ILanguageService
{
    private readonly ILogger<LanguageService> _logger;

    public LanguageService(ILogger<LanguageService> logger)
    {
        _logger = logger;
    }

    public void InitializeLanguage()
    {
        var languageCode = GetCurrentLanguage();
        SetLanguage(languageCode);

        _logger.LogInformation("[LanguageService] - Language initialized to: {Language}", languageCode);
    }

    public string GetCurrentLanguage()
    {
        var savedLanguage = Preferences.Default.Get("app_language", "es");

        _logger.LogInformation("[LanguageService] - Current language from storage: {Language}", savedLanguage);

        return savedLanguage;
    }

    /// <summary>
    /// Change the application language and update UI
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        try
        {
            _logger.LogInformation("[LanguageService] - Setting language to: {Code}", languageCode);

            var nextCulture = languageCode switch
            {
                "en" => new CultureInfo("en-US"),
                "sv" => new CultureInfo("sv-SE"),
                _ => new CultureInfo("es-CL")
            };

            LocalizationResourceManager.Instance.Culture = nextCulture;
            SaveLanguage(languageCode);

            _logger.LogInformation("[LanguageService] - Language set successfully to: {Culture}", nextCulture.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LanguageService] - Error setting language: {Code}", languageCode);
        }
    }

    /// <summary>
    /// Extract language code from selection text
    /// </summary>
    public string ExtractLanguageCode(string selection)
    {
        var code = selection switch
        {
            "🇬🇧 UK - English" => SupportedLanguages.English,
            "🇸🇪 SE - Svenska" => SupportedLanguages.Swedish,
            _ => SupportedLanguages.Spanish
        };

        _logger.LogInformation("[LanguageService] - Extracted code: {Code}", code);
        return code;
    }

    public void SaveLanguage(string languageCode)
    {
        Preferences.Default.Set("app_language", languageCode);

        _logger.LogInformation("[LanguageService] - Language saved to storage: {Code}", languageCode);
    }
}
