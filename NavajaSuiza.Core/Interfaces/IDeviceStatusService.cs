namespace NavajaSuiza.Core.Interfaces;

public interface IDeviceStatusService
{
    string GetBatteryLevel();
    string GetAvailableStorage();
}
