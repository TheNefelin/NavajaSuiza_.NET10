using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILanguageService _languageService;
    private readonly IThemeService _themeService;
    private readonly ILogger<App> _logger;

    public App(
        IServiceProvider serviceProvider, 
        ILanguageService languageService,
        IThemeService themeService,
        ILogger<App> logger)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _languageService = languageService;
        _themeService = themeService;
        _logger = logger;

        SetupGlobalErrorHandling();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override void OnStart()
    {
        _languageService.InitializeLanguage();
        _themeService.ApplySavedTheme();
    }

    private void SetupGlobalErrorHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            _logger.LogCritical(exception, "Unhandled exception terminó la app: {Message}", exception?.Message);
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            _logger.LogError(args.Exception, "Unobserved task exception: {Message}", args.Exception.Message);
            args.SetObserved();
        };
    }
}