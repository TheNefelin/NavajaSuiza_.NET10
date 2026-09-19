using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class DeviceStatusService : IDeviceStatusService
{
    public string GetBatteryLevel()
    {
        try
        {
#if ANDROID
            var batteryManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.BatteryService) as Android.OS.BatteryManager;
            var capacity = batteryManager?.GetIntProperty((int)Android.OS.BatteryProperty.Capacity);
            if (capacity is >= 0 and <= 100)
                return $"{capacity}%";
#endif
            double level = Battery.Default.ChargeLevel;
            if (level > 0 && level <= 1)
                return $"{(int)(level * 100)}%";
        }
        catch
        {
            return "N/A";
        }

        return "N/A";
    }

    public string GetAvailableStorage()
    {
        try
        {
#if ANDROID
            var path = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
            if (!string.IsNullOrEmpty(path))
            {
                var statFs = new Android.OS.StatFs(path);
                long availableBytes = statFs.AvailableBytes;
                return FormatBytes(availableBytes);
            }
#endif
            return "N/A";
        }
        catch
        {
            return "N/A";
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}