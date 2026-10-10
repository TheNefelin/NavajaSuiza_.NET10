using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.Services;

public class DeviceStatusService : IDeviceStatusService
{
    private readonly IDeviceStatusSource _source;
    private readonly ILogger<DeviceStatusService> _logger;

    public DeviceStatusService(IDeviceStatusSource source, ILogger<DeviceStatusService> logger)
    {
        _source = source;
        _logger = logger;
    }

    public int GetBatteryCapacity()
    {
        try
        {
            var capacity = _source.GetBatteryCapacity();
            if (capacity is >= 0 and <= 100)
                return capacity;

            _logger.LogInformation("BatteryManager.Capacity devolvio {Capacity}, se usa la API de Essentials", capacity);

            var level = _source.GetBatteryChargeLevel();
            if (level >= 0 && level <= 1)
                return (int)(level * 100);

            _logger.LogInformation("Battery.Default.ChargeLevel devolvio {Level}", level);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer la carga de la bateria");
        }

        return -1;
    }

    public long GetAvailableStorageBytes()
    {
        try
        {
            return _source.GetAvailableStorageBytes();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el almacenamiento disponible");
            return -1;
        }
    }

    public long GetTotalStorageBytes()
    {
        try
        {
            return _source.GetTotalStorageBytes();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el almacenamiento total");
            return -1;
        }
    }
}
