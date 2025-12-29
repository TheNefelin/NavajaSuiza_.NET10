using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILanguageService _languageService;

    public App(
        IServiceProvider serviceProvider, ILanguageService languageService)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _languageService = languageService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override async void OnStart()
    {
        _languageService.InitializeLanguage();
    }
}