using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Implementations;
using System.Globalization;

namespace NavajaSuiza_.NET10;

/// <summary>
/// AppShell handles the main shell layout and language switching functionality
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Property that provides access to the LocalizationResourceManager singleton instance
    /// This allows XAML binding to access translated resources
    /// </summary>
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance;
    private readonly ILogger<AppShell> _logger;

    /// <summary>
    /// Constructor - Initializes the AppShell and sets the BindingContext
    /// BindingContext = this allows XAML to bind to this code-behind class
    /// </summary>
    public AppShell(ILogger<AppShell> logger)
    {
        InitializeComponent();
        _logger = logger;

        // Set the BindingContext to this class so XAML can access LocalizationResourceManager property
        BindingContext = this;

        _logger.LogInformation("[AppShell] - AppShell initialized");
    }

    /// <summary>
    /// Handle language selection when user clicks the language toolbar button
    /// Displays an ActionSheet with 3 language options
    /// </summary>
    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        _logger.LogInformation("[AppShell] - Language button clicked");

        // Display ActionSheet with language options
        string result = await DisplayActionSheetAsync(
            "Select Language",
            "Cancel",
            null,
            "🇨🇱 CL - Español",
            "🇬🇧 UK - English",
            "🇸🇪 SE - Svenska");

        // If user selected a language (not Cancel)
        if (result != null && result != "Cancel")
        {
            _logger.LogInformation("[AppShell] - User selected language: {Language}", result);

            // Extract the language code from the user selection
            string languageCode = ExtractLanguageCode(result);
            _logger.LogInformation("[AppShell] - Extracted language code: {Code}", languageCode);

            // Update the toolbar item (flag and text)
            UpdateToolbarItem(languageCode);
        }
        else
        {
            _logger.LogInformation("[AppShell] - User cancelled language selection");
        }
    }

    /// <summary>
    /// Convert the user-selected language option to a language code
    /// Maps display text to ISO language codes (es, en, sv)
    /// </summary>
    /// <param name="selection">The text selected from the ActionSheet</param>
    /// <returns>Language code (es, en, or sv)</returns>
    private string ExtractLanguageCode(string selection)
    {
        var code = selection switch
        {
            "🇬🇧 UK - English" => SupportedLanguages.English,    // "en"
            "🇸🇪 SE - Svenska" => SupportedLanguages.Swedish,    // "sv"
            _ => SupportedLanguages.Spanish                     // "es" (default)
        };

        _logger.LogInformation("[AppShell] - ExtractLanguageCode returned: {Code}", code);

        return code;
    }

    /// <summary>
    /// Update the toolbar item with the selected language's culture and flag
    /// This method changes the app's language and updates the UI
    /// </summary>
    /// <param name="languageCode">The language code to switch to (es, en, or sv)</param>
    private void UpdateToolbarItem(string languageCode)
    {
        try
        {
            _logger.LogInformation("[AppShell] - Updating toolbar item to language code: {Code}", languageCode);

            // Convert language code to CultureInfo object
            // CultureInfo determines which .resx file to use for translations
            var nextCulture = languageCode switch
            {
                "en" => new CultureInfo("en-US"),      // English (United States)
                "sv" => new CultureInfo("sv-SE"),      // Swedish (Sweden)
                _ => new CultureInfo("es-CL")          // Spanish (Chile) - default
            };

            _logger.LogInformation("[AppShell] - Setting culture to: {Culture}", nextCulture.Name);

            // Set the new culture globally in LocalizationResourceManager
            // This triggers INotifyPropertyChanged event, updating all bound UI elements
            LocalizationResourceManager.Instance.Culture = nextCulture;

            _logger.LogInformation("[AppShell] - Culture set successfully to: {Culture}", nextCulture.Name);

            // Update the toolbar item to show the selected flag and language code
            var toolbarItem = this.ToolbarItems.FirstOrDefault();
            if (toolbarItem != null)
            {
                // Set the flag emoji and country code based on selected language
                toolbarItem.Text = languageCode switch
                {
                    "en" => "🇬🇧 UK",     // English flag and code
                    "sv" => "🇸🇪 SE",     // Swedish flag and code
                    _ => "🇨🇱 CL"         // Spanish flag and code (default)
                };

                _logger.LogInformation("[AppShell] - Toolbar item updated to: {Text}", toolbarItem.Text);
            }
            else
            {
                _logger.LogWarning("[AppShell] - ToolbarItem not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppShell] - Error updating toolbar item for language code: {Code}", languageCode);
            _logger.LogError("[AppShell] - Exception Message: {Message}", ex.Message);
            _logger.LogError("[AppShell] - Stack Trace: {StackTrace}", ex.StackTrace);
        }
    }
}
