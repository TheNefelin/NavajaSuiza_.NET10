using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class ScreenLightViewModel : BaseViewModel
{
#if ANDROID
    private double _originalBrightness = 0.5;
#endif

    public void InitializeScreenLight()
    {
#if ANDROID
        _originalBrightness = GetCurrentBrightness();
        SetScreenBrightness(1.0);
#endif
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    public override void Cleanup()
    {
        base.Cleanup();
#if ANDROID
        SetScreenBrightness(_originalBrightness);
#endif
        DeviceDisplay.Current.KeepScreenOn = false;
    }

#if ANDROID
    private static double GetCurrentBrightness()
    {
        var window = Platform.CurrentActivity?.Window;
        return window?.Attributes?.ScreenBrightness ?? 0.5;
    }
#endif

#if ANDROID
    private static void SetScreenBrightness(double brightness)
    {
        var window = Platform.CurrentActivity?.Window;
        window?.Attributes?.ScreenBrightness = (float)brightness;
        if (window?.Attributes != null)
        {
            window.Attributes = window.Attributes;
        }
    }
#endif
}
