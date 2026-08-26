using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza_.NET10.ViewModels;
using NavajaSuiza_.NET10.Services.Implementations;
using NavajaSuiza.Core.Interfaces;
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
            .UseMauiCommunityToolkitMediaElement(false)
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
            .AddSingleton<IMetronomeService, MetronomeService>()
            ;

        builder.Services
            .AddTransient<TestingViewModel>()
            .AddSingleton<TestingPage>()
            ;

        builder.Services
            .AddSingleton<AppShell>()
            .AddTransient<MenuViewModel>()
            .AddSingleton<MenuPage>()
            .AddTransient<ManualViewModel>()
            .AddSingleton<ManualPage>()
            .AddTransient<AboutViewModel>()
            .AddSingleton<AboutPage>()
            .AddTransient<FlashlightViewModel>()
            .AddSingleton<FlashlightPage>()
            .AddTransient<ScreenLightViewModel>()
            .AddSingleton<ScreenLightPage>()
            .AddTransient<TunerViewModel>()
            .AddSingleton<TunerPage>()
            .AddTransient<InstrumentViolinViewModel>()
            .AddSingleton<InstrumentViolinPage>()
            .AddTransient<InstrumentNylonViewModel>()
            .AddSingleton<InstrumentNylonPage>()
            .AddTransient<InstrumentSteelViewModel>()
            .AddSingleton<InstrumentSteelPage>()
            .AddTransient<InstrumentBassViewModel>()
            .AddSingleton<InstrumentBassPage>()
            .AddTransient<InstrumentUkuleleViewModel>()
            .AddSingleton<InstrumentUkulelePage>()
            .AddTransient<InstrumentCharangoViewModel>()
            .AddSingleton<InstrumentCharangoPage>()
            .AddTransient<MetronomeViewModel>()
            .AddSingleton<MetronomePage>()
            .AddTransient<FramingViewModel>()
            .AddSingleton<FramingPage>()
            .AddTransient<CompassViewModel>()
            .AddSingleton<CompassPage>()
            ;

        return builder;
    }
}