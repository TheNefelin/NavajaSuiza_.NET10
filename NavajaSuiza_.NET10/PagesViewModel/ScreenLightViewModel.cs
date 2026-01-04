namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class ScreenLightViewModel : BaseViewModel
{
    private double _originalBrightness = 0.5;

    public void InitializeScreenLight()
    {
//#if ANDROID
//        _originalBrightness = GetCurrentBrightness();
//        SetScreenBrightness(1.0);
//#endif
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    public void Cleanup()
    {
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
        if (window != null)
        {
            var params_ = window.Attributes;
            params_.ScreenBrightness = (float)brightness;
            window.Attributes = params_;
        }
    }
#endif
}
