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
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureServices(); ;

        //ConfigureStatusBar();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure =>
            configure.AddDebug().SetMinimumLevel(LogLevel.Information));
#endif


        return builder.Build();
    }

    private static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services
            .AddSingleton<ILanguageService, LanguageService>()
            .AddSingleton<IThemeService, ThemeService>();

        builder.Services
            .AddSingleton<AppShell>()
            .AddSingleton<AboutPage>()
            .AddSingleton<AboutViewModel>();

        return builder;
    }

    private static void ConfigureStatusBar()
    {
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if ANDROID
            var color = Android.Graphics.Color.ParseColor("#E4E4E5");

            handler.PlatformView.Window?.SetStatusBarColor(color);
            handler.PlatformView.Window?.SetNavigationBarColor(color);
#endif

#if IOS
#endif
        });
    }
}
