using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza_.NET10.PagesViewModel;
using NavajaSuiza_.NET10.Services.Implementations;
using NavajaSuiza_.NET10.Services.Interfaces;
using Syncfusion.Maui.Toolkit.Hosting;

namespace NavajaSuiza_.NET10;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureServices();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure =>
            configure
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace));
#else
    // En Release también queremos logs
    builder.Services.AddLogging(configure =>
        configure
            .AddDebug()
            .SetMinimumLevel(LogLevel.Information));
#endif

        return builder.Build();
    }

    private static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<ILanguageService, LanguageService>()
            .AddSingleton<IThemeService, ThemeService>()
            .AddSingleton<IDeviceStatusService, DeviceStatusService>()
            .AddSingleton<IInstrumentAudioService, InstrumentAudioService>()
            ;

        builder.Services
            .AddSingleton<TestingViewModel>()
            .AddSingleton<TestingPage>()
            ;

        builder.Services
            .AddSingleton<AppShell>()
            .AddSingleton<MenuViewModel>()
            .AddSingleton<MenuPage>()
            .AddSingleton<ManualViewModel>()
            .AddSingleton<ManualPage>()
            .AddSingleton<AboutViewModel>()
            .AddSingleton<AboutPage>()
            .AddSingleton<FlashlightViewModel>()
            .AddSingleton<FlashlightPage>()
            .AddSingleton<ScreenLightViewModel>()
            .AddSingleton<ScreenLightPage>()
            .AddSingleton<TunerViewModel>()
            .AddSingleton<TunerPage>()
            ;

        return builder;
    }
}