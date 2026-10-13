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
        LogStartupInfo();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _themeService.ApplySavedTheme();
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override void OnStart()
    {
        _languageService.InitializeLanguage();
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

#if ANDROID
        // En Android las excepciones no controladas pasan por este hook (además del de AppDomain).
        // No se marca args.Handled: la intención es registrar, no alterar el comportamiento del runtime.
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            _logger.LogCritical(args.Exception, "Unhandled Android exception: {Message}", args.Exception.Message);
        };
#endif
    }

    private void LogStartupInfo()
    {
        _logger.LogInformation(
            "Navaja Suiza v{Version} (build {Build}) en {Platform} {PlatformVersion}, modelo {Model}",
            AppInfo.Current.VersionString,
            AppInfo.Current.BuildString,
            DeviceInfo.Current.Platform,
            DeviceInfo.Current.VersionString,
            DeviceInfo.Current.Model);
    }
}