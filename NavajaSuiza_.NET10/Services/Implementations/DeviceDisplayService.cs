using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class DeviceDisplayService : IDeviceDisplayService
{
    public bool KeepScreenOn
    {
        get => DeviceDisplay.Current.KeepScreenOn;
        set => DeviceDisplay.Current.KeepScreenOn = value;
    }
}
