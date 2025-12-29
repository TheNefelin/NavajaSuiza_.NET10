namespace NavajaSuiza_.NET10;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(appShell);
    }

    protected override async void OnStart()
    {
        // Initialize language when app starts
    }
}