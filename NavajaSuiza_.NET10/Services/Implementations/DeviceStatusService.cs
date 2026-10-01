using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class DeviceStatusService : IDeviceStatusService
{
    private readonly ILogger<DeviceStatusService> _logger;

    public DeviceStatusService(ILogger<DeviceStatusService> logger)
    {
        _logger = logger;
    }

    public int GetBatteryCapacity()
    {
        try
        {
#if ANDROID
            var batteryManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.BatteryService) as Android.OS.BatteryManager;
            var capacity = batteryManager?.GetIntProperty((int)Android.OS.BatteryProperty.Capacity) ?? -1;
            if (capacity is >= 0 and <= 100)
                return capacity;

            _logger.LogInformation("BatteryManager.Capacity devolvio {Capacity}, se usa la API de Essentials", capacity);
#endif
            double level = Battery.Default.ChargeLevel;
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
#if ANDROID
            return GetStatFs().AvailableBytes;
#else
            return GetDrive().AvailableFreeSpace;
#endif
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
#if ANDROID
            return GetStatFs().TotalBytes;
#else
            return GetDrive().TotalSize;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer el almacenamiento total");
            return -1;
        }
    }

#if ANDROID
    private Android.OS.StatFs GetStatFs()
    {
        var path = Android.OS.Environment.DataDirectory?.AbsolutePath ?? string.Empty;
        if (string.IsNullOrEmpty(path))
        {
            _logger.LogWarning("No se encontro una ruta de almacenamiento valida");
            return new Android.OS.StatFs(Android.OS.Environment.RootDirectory?.AbsolutePath ?? "/");
        }

        return new Android.OS.StatFs(path);
    }
#else
    private DriveInfo GetDrive()
    {
        var path = FileSystem.Current.AppDataDirectory;
        if (string.IsNullOrEmpty(path))
        {
            _logger.LogWarning("No se encontro una ruta de almacenamiento valida");
            return new DriveInfo(Path.GetTempPath());
        }

        return new DriveInfo(path);
    }
#endif
}