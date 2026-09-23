using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Pages;
using NavajaSuiza_.NET10.Services.Implementations;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;
using NavajaSuiza.Core.ViewModels;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace NavajaSuiza_.NET10;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Reemplaza el placeholder por tu clave de licencia de Syncfusion (Community License, gratuita:
        // https://www.syncfusion.com/products/communitylicense). No compartas la clave en repositorios públicos.
        SyncfusionLicenseProvider.RegisterLicense("CLAVE_LICENCIA_SYNCFUSION");

        var builder = MauiApp.CreateBuilder();
#pragma warning disable CA1416 // CommunityToolkit.Maui.MediaElement requires Android 26+; min SDK stays at 21 for broader device support
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement(false)
            .ConfigureSyncfusionToolkit()
            .ConfigureSyncfusionCore()
#pragma warning restore CA1416
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
            .AddTransient<IInstrumentAudioService, InstrumentAudioService>()
            .AddTransient<IMetronomeService, MetronomeService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<ICompassService, CompassSensorService>()
            .AddSingleton<IOrientationService, OrientationSensorService>()
            .AddSingleton<IFlashlightService, FlashlightService>()
            .AddSingleton<IDeviceDisplayService, DeviceDisplayService>()
            .AddSingleton<IImagePickerService, ImagePickerService>()
            .AddSingleton<IScreenBrightnessService, ScreenBrightnessService>()
.AddSingleton<IFlashlightStateService, FlashlightStateService>()
            .AddSingleton<IStopwatchService, StopwatchService>()
            .AddTransient<IMorseSignalService, MorseSignalService>()
            .AddSingleton<IAppInfoService, AppInfoService>()
            .AddSingleton<ILauncherService, LauncherService>()
            .AddSingleton<INotesRepository, SqliteNotesRepository>()
            .AddSingleton<IPizarraImageExporter, PizarraImageExporter>()
            .AddSingleton<IFilePickerService, FilePickerService>()
            .AddSingleton<IDocumentPdfConverter, DocumentPdfConverter>()
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
            .AddTransient<StopwatchViewModel>()
            .AddSingleton<StopwatchPage>()
            .AddTransient<FramingViewModel>()
            .AddSingleton<FramingPage>()
.AddTransient<CompassViewModel>()
            .AddSingleton<CompassPage>()
            .AddTransient<NotesViewModel>()
            .AddSingleton<NotesPage>()
            .AddTransient<NoteEditorViewModel>()
            .AddSingleton<NoteEditorPage>()
            .AddTransient<PizarraViewModel>()
            .AddSingleton<PizarraPage>()
            .AddTransient<PdfReaderViewModel>()
            .AddTransient<PdfReaderPage>()
            .AddTransient<TextReaderViewModel>()
            .AddTransient<TextReaderPage>()
            ;

        return builder;
    }
}
