using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class ScreenLightViewModel : BaseViewModel
{
    private readonly IDeviceDisplayService _deviceDisplayService;
    private readonly IScreenBrightnessService _brightnessService;
    private double _originalBrightness = 0.5;

    public ScreenLightViewModel(
        IDeviceDisplayService deviceDisplayService,
        IScreenBrightnessService brightnessService)
    {
        _deviceDisplayService = deviceDisplayService;
        _brightnessService = brightnessService;
    }

    public void InitializeScreenLight()
    {
        _originalBrightness = _brightnessService.GetCurrentBrightness();
        _brightnessService.SetScreenBrightness(1.0);
        _deviceDisplayService.KeepScreenOn = true;
    }

    public override void Cleanup()
    {
        base.Cleanup();
        _brightnessService.SetScreenBrightness(_originalBrightness);
        _deviceDisplayService.KeepScreenOn = false;
    }
}
