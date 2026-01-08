using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10;

public partial class AppShell : Shell
{
    private readonly ILogger<AppShell> _logger;
    private readonly ILanguageService _languageService;

    public AppShell(
        ILogger<AppShell> logger,
        ILanguageService languageService)
    {
        InitializeComponent();
        _logger = logger;
        _languageService = languageService;

        Shell.SetTabBarIsVisible(this, true);

        UpdateToolbarForCurrentLanguage();
    }

    private void UpdateToolbarForCurrentLanguage()
    {
        var currentLanguage = _languageService.GetCurrentLanguage();

        var toolbarItem = ToolbarItems.FirstOrDefault();
        if (toolbarItem != null)
        {
            toolbarItem.Text = currentLanguage switch
            {
                "en" => "🇬🇧 UK",
                "sv" => "🇸🇪 SE",
                _ => "🇨🇱 CL"
            };

            _logger.LogInformation("[AppShell] - Toolbar initialized to: {Text}", toolbarItem.Text);
        }
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        _logger.LogInformation("[AppShell] - Language button clicked");

        var selectLanguage = LocalizationResourceManager.Instance["SelectLanguageText"].ToString();
        var cancelText = LocalizationResourceManager.Instance["CancelText"]?.ToString();

        string result = await DisplayActionSheetAsync(
            selectLanguage,
            cancelText,
            null,
            "🇨🇱 CL - Español",
            "🇬🇧 UK - English",
            "🇸🇪 SE - Svenska");

        if (result != null && result != cancelText)
        {
            _logger.LogInformation("[AppShell] - User selected: {Language}", result);

            string languageCode = _languageService.ExtractLanguageCode(result);
            _languageService.SetLanguage(languageCode);
            UpdateToolbarItem(languageCode);
        }
    }

    private void UpdateToolbarItem(string languageCode)
    {
        var toolbarItem = this.ToolbarItems.FirstOrDefault();
        if (toolbarItem != null)
        {
            toolbarItem.Text = languageCode switch
            {
                "en" => "🇬🇧 UK",
                "sv" => "🇸🇪 SE",
                _ => "🇨🇱 CL"
            };

            _logger.LogInformation("[AppShell] - Toolbar updated to: {Text}", toolbarItem.Text);
        }
    }

    public static async Task DisplayToastAsync(string message)
    {
        // Toast is currently not working in MCT on Windows
        if (OperatingSystem.IsWindows())
            return;

        var toast = Toast.Make(message, textSize: 18);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await toast.Show(cts.Token);
    }
}
