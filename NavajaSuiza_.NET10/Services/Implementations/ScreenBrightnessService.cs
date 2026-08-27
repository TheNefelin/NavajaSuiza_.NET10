using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ScreenBrightnessService : IScreenBrightnessService
{
    public double GetCurrentBrightness()
    {
#if ANDROID
        var window = Platform.CurrentActivity?.Window;
        return window?.Attributes?.ScreenBrightness ?? 0.5;
#else
        return 0.5;
#endif
    }

    public void SetScreenBrightness(double brightness)
    {
#if ANDROID
        var window = Platform.CurrentActivity?.Window;
        if (window?.Attributes != null)
        {
            window.Attributes.ScreenBrightness = (float)brightness;
            window.Attributes = window.Attributes;
        }
#endif
    }
}
