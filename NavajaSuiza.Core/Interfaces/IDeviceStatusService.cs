namespace NavajaSuiza.Core.Interfaces;

public interface IDeviceStatusService
{
    int GetBatteryCapacity();
    long GetAvailableStorageBytes();
    long GetTotalStorageBytes();
}