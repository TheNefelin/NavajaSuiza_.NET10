using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

/// <summary>
/// Lecturas crudas de batería y almacenamiento del dispositivo. Solo esta capa conoce las APIs
/// de plataforma; la lógica de validación vive en <see cref="NavajaSuiza.Core.Services.DeviceStatusService"/>.
/// </summary>
public class PlatformDeviceStatusSource : IDeviceStatusSource
{
    private readonly ILogger<PlatformDeviceStatusSource> _logger;

    public PlatformDeviceStatusSource(ILogger<PlatformDeviceStatusSource> logger)
    {
        _logger = logger;
    }

    public int GetBatteryCapacity()
    {
#if ANDROID
        var batteryManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.BatteryService) as Android.OS.BatteryManager;
        return batteryManager?.GetIntProperty((int)Android.OS.BatteryProperty.Capacity) ?? -1;
#else
        return -1;
#endif
    }

    public double GetBatteryChargeLevel() => Battery.Default.ChargeLevel;

    public long GetAvailableStorageBytes()
    {
#if ANDROID
        return GetStatFs().AvailableBytes;
#else
        return GetDrive().AvailableFreeSpace;
#endif
    }

    public long GetTotalStorageBytes()
    {
#if ANDROID
        return GetStatFs().TotalBytes;
#else
        return GetDrive().TotalSize;
#endif
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
