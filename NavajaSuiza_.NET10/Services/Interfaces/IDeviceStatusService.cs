namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface IDeviceStatusService
{
    string GetBatteryLevel();
    string GetAvailableStorage();
}
