using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILanguageService _languageService;
    private readonly IThemeService _themeService;

    public App(
        IServiceProvider serviceProvider, 
        ILanguageService languageService,
        IThemeService themeService)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _languageService = languageService;
        _themeService = themeService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override async void OnStart()
    {
        _languageService.InitializeLanguage();
        _themeService.ApplySavedTheme();
    }
}