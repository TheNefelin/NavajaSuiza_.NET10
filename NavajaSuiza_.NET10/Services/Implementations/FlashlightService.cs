using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class FlashlightService : IFlashlightService
{
    private const int MinimumIntensityLevel = 1;

    private int _intensityLevel = MinimumIntensityLevel;

#if ANDROID
    private readonly Android.Hardware.Camera2.CameraManager? _cameraManager;
    private readonly string? _torchCameraId;
    private readonly int _maxIntensityLevel;
    private readonly int _defaultIntensityLevel;
    private bool _isTorchOn;
#endif

    public FlashlightService()
    {
#if ANDROID
        _cameraManager = ResolveCameraManager();
        _torchCameraId = ResolveTorchCameraId(_cameraManager);
        _maxIntensityLevel = _torchCameraId is null
            ? 0
            : ResolveMaximumIntensityLevel(_cameraManager!, _torchCameraId);
        _defaultIntensityLevel = _torchCameraId is null
            ? 0
            : ResolveDefaultIntensityLevel(_cameraManager!, _torchCameraId);
#endif
        _intensityLevel = ResolveInitialIntensity();
    }

    public bool SupportsVariableIntensity
    {
        get
        {
#if ANDROID
            return _torchCameraId is not null;
#else
            return false;
#endif
        }
    }

    public int MaxIntensityLevel
    {
        get
        {
#if ANDROID
            return _maxIntensityLevel;
#else
            return 0;
#endif
        }
    }

    public int IntensityLevel
    {
        get => _intensityLevel;
        set
        {
            var level = NormalizeIntensity(value);

            if (level == _intensityLevel)
                return;

            _intensityLevel = level;

#if ANDROID
            if (_isTorchOn)
                TryApplyIntensity();
#endif
        }
    }

    public Task TurnOnAsync()
    {
#if ANDROID
        return TurnOnTorchAsync();
#else
        return Flashlight.Default.TurnOnAsync();
#endif
    }

    public Task TurnOffAsync()
    {
#if ANDROID
        return TurnOffTorchAsync();
#else
        return Flashlight.Default.TurnOffAsync();
#endif
    }

    private int ResolveInitialIntensity()
    {
        var maximum = MaxIntensityLevel;

        if (maximum <= MinimumIntensityLevel)
            return MinimumIntensityLevel;

#if ANDROID
        return _defaultIntensityLevel < MinimumIntensityLevel
            ? maximum
            : Math.Clamp(_defaultIntensityLevel, MinimumIntensityLevel, maximum);
#else
        return MinimumIntensityLevel;
#endif
    }

    private int NormalizeIntensity(int level)
    {
        var maximum = MaxIntensityLevel;

        if (maximum <= MinimumIntensityLevel)
            return MinimumIntensityLevel;

        return Math.Clamp(level, MinimumIntensityLevel, maximum);
    }

#if ANDROID
    private async Task TurnOnTorchAsync()
    {
        if (TryApplyIntensity())
        {
            _isTorchOn = true;
            return;
        }

        await Flashlight.Default.TurnOnAsync();
        _isTorchOn = true;
    }

    private async Task TurnOffTorchAsync()
    {
        if (TryDisableTorch())
        {
            _isTorchOn = false;
            return;
        }

        await Flashlight.Default.TurnOffAsync();
        _isTorchOn = false;
    }

    private bool TryApplyIntensity()
    {
        if (_torchCameraId is null || _cameraManager is null || !OperatingSystem.IsAndroidVersionAtLeast(33))
            return false;

        _cameraManager.TurnOnTorchWithStrengthLevel(_torchCameraId, _intensityLevel);
        return true;
    }

    private bool TryDisableTorch()
    {
        if (_torchCameraId is null || _cameraManager is null || !OperatingSystem.IsAndroidVersionAtLeast(23))
            return false;

        _cameraManager.SetTorchMode(_torchCameraId, false);
        return true;
    }

    private static Android.Hardware.Camera2.CameraManager? ResolveCameraManager() =>
        Android.App.Application.Context
            .GetSystemService(Android.Content.Context.CameraService)
            as Android.Hardware.Camera2.CameraManager;

    private static string? ResolveTorchCameraId(Android.Hardware.Camera2.CameraManager? cameraManager)
    {
        if (cameraManager is null)
            return null;

        try
        {
            foreach (var cameraId in cameraManager.GetCameraIdList())
            {
                if (ResolveMaximumIntensityLevel(cameraManager, cameraId) > MinimumIntensityLevel)
                    return cameraId;
            }
        }
        catch (Java.Lang.Exception)
        {
            return null;
        }

        return null;
    }

    private static int ResolveMaximumIntensityLevel(
        Android.Hardware.Camera2.CameraManager cameraManager,
        string cameraId)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return 0;

        try
        {
            return ReadLevelKey(
                cameraManager,
                cameraId,
                Android.Hardware.Camera2.CameraCharacteristics.FlashInfoStrengthMaximumLevel);
        }
        catch (Java.Lang.Exception)
        {
            return 0;
        }
    }

    private static int ResolveDefaultIntensityLevel(
        Android.Hardware.Camera2.CameraManager cameraManager,
        string cameraId)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return 0;

        try
        {
            return ReadLevelKey(
                cameraManager,
                cameraId,
                Android.Hardware.Camera2.CameraCharacteristics.FlashInfoStrengthDefaultLevel);
        }
        catch (Java.Lang.Exception)
        {
            return 0;
        }
    }

    private static int ReadLevelKey(
        Android.Hardware.Camera2.CameraManager cameraManager,
        string cameraId,
        Android.Hardware.Camera2.CameraCharacteristics.Key key)
    {
        var level = cameraManager
            .GetCameraCharacteristics(cameraId)
            .Get(key);

        return level is Java.Lang.Integer boxedLevel
            ? boxedLevel.IntValue()
            : 0;
    }
#endif
}