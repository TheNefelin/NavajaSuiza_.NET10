using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class CompassViewModel : BaseViewModel, IDisposable
{
    private readonly ILogger<CompassViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly INavigationService _navigationService;
    private readonly ICompassService _compassService;
    private readonly IOrientationService _orientationService;
    private readonly ICompassPositionService _compassPositionService;

    private const double SmoothingFactor = 0.3;
    private double _smoothedHeading = -1;
    private double _declinationDegrees;
    private bool _hasDeclination;

    private string[]? _cardinalDirections16;

    private CancellationTokenSource? _calibrationCts;
    private CancellationTokenSource? _positionCts;

    [ObservableProperty]
    public partial string StatusText { get; set; } = "...";

    [ObservableProperty]
    public partial string AngleText { get; set; } = "0°";

    [ObservableProperty]
    public partial string CardinalDirection { get; set; } = "N/A";

    [ObservableProperty]
    public partial double CompassDialRotation { get; set; } = 0.0;

    [ObservableProperty]
    public partial bool IsCalibrating { get; set; }

    [ObservableProperty]
    public partial string CalibrationText { get; set; } = "...";

    [ObservableProperty]
    public partial bool IsLocating { get; set; }

    [ObservableProperty]
    public partial bool HasPosition { get; set; }

    [ObservableProperty]
    public partial bool IsLocationDisabledMessage { get; set; }

    [ObservableProperty]
    public partial string LatitudeText { get; set; } = "--";

    [ObservableProperty]
    public partial string LongitudeText { get; set; } = "--";

    [ObservableProperty]
    public partial string AltitudeText { get; set; } = "--";

    [ObservableProperty]
    public partial string AccuracyText { get; set; } = "--";

    [ObservableProperty]
    public partial string PositionMessage { get; set; } = string.Empty;

    public CompassViewModel(
        ILogger<CompassViewModel> logger,
        ILanguageService languageService,
        INavigationService navigationService,
        ICompassService compassService,
        IOrientationService orientationService,
        ICompassPositionService compassPositionService)
    {
        _logger = logger;
        _languageService = languageService;
        _navigationService = navigationService;
        _compassService = compassService;
        _orientationService = orientationService;
        _compassPositionService = compassPositionService;

        _languageService.LanguageChanged += OnLanguageChanged;
        BuildCardinalDirections();
    }

    [RelayCommand]
    public async Task StartSensorsAsync()
    {
        if (!_compassService.IsSupported || !_orientationService.IsSupported)
        {
            _logger.LogWarning("Navigating back due to unsupported sensors");
            await _navigationService.DisplayAlertAsync("Error", "Los sensores necesarios no son soportados en este dispositivo.", "OK");
            await _navigationService.GoToAsync("..");
            return;
        }

        _compassService.ReadingChanged -= OnCompassReadingChanged;
        _compassService.ReadingChanged += OnCompassReadingChanged;
        _compassService.Start(applyLowPassFilter: true);

        _orientationService.ReadingChanged -= OnOrientationReadingChanged;
        _orientationService.ReadingChanged += OnOrientationReadingChanged;
        _orientationService.Start();
    }

    [RelayCommand]
    private async Task CalibrateAsync()
    {
        if (IsCalibrating) return;

        _calibrationCts?.Cancel();
        _calibrationCts = new CancellationTokenSource();
        var token = _calibrationCts.Token;

        IsCalibrating = true;

        try
        {
            CalibrationText = _languageService.GetString("CompassCalibrationStartText") ?? "Mover el dispositivo en forma de 8";
            await Task.Delay(3000, token);

            if (token.IsCancellationRequested) return;

            CalibrationText = _languageService.GetString("CompassCalibrationMiddleText") ?? "Calibrando...";
            await Task.Delay(3000, token);

            if (token.IsCancellationRequested) return;

            CalibrationText = _languageService.GetString("CompassCalibrationEndText") ?? "Calibración completada";
            await Task.Delay(1000, token);

            IsCalibrating = false;
        }
        catch (TaskCanceledException)
        {
            IsCalibrating = false;
        }
    }

    [RelayCommand]
    public void StopSensors()
    {
        _calibrationCts?.Cancel();
        _positionCts?.Cancel();
        IsCalibrating = false;
        _smoothedHeading = -1;

        _compassService.ReadingChanged -= OnCompassReadingChanged;
        _compassService.Stop();

        _orientationService.ReadingChanged -= OnOrientationReadingChanged;
        _orientationService.Stop();
    }

    [RelayCommand]
    private async Task LocateAsync()
    {
        if (IsLocating)
            return;

        _positionCts?.Cancel();
        _positionCts = new CancellationTokenSource();
        var token = _positionCts.Token;

        IsLocating = true;
        IsLocationDisabledMessage = false;
        PositionMessage = GetString("CompassLocatingText", "Buscando señal...");

        try
        {
            if (!_compassPositionService.IsAvailable)
            {
                IsLocationDisabledMessage = true;
                PositionMessage = GetString(
                    "CompassLocationDisabledText",
                    "Debes activar la Ubicación en el dispositivo.");
                return;
            }

            var reading = await _compassPositionService.GetCurrentPositionAsync(token);

            var unavailableText = GetString("CompassValueUnavailableText", "no disponible");
            LatitudeText = FormatCoordinate(reading.Latitude, isLatitude: true);
            LongitudeText = FormatCoordinate(reading.Longitude, isLatitude: false);
            AltitudeText = reading.AltitudeMeters is > 0
                ? string.Create(CultureInfo.InvariantCulture, $"{reading.AltitudeMeters:F0} m")
                : unavailableText;
            AccuracyText = reading.HorizontalAccuracyMeters is > 0
                ? string.Create(
                    CultureInfo.InvariantCulture,
                    $"≈{reading.HorizontalAccuracyMeters:F0} m")
                : unavailableText;

            HasPosition = true;
            IsLocationDisabledMessage = false;
            PositionMessage = string.Empty;

            UpdateDeclination(reading.Latitude, reading.Longitude, reading.AltitudeMeters);
        }
        catch (OperationCanceledException)
        {
            IsLocationDisabledMessage = false;
            PositionMessage = string.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            IsLocationDisabledMessage = false;
            PositionMessage = GetString(
                "CompassPermissionDeniedText",
                "Se necesita permiso de ubicación para mostrar la posición.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not read the device position");
            IsLocationDisabledMessage = false;
            PositionMessage = GetString(
                "CompassPositionErrorText",
                "No se pudo obtener la posición. Salí al exterior e intenta de nuevo.");
        }
        finally
        {
            IsLocating = false;
        }
    }

    private string GetString(string key, string fallback) =>
        _languageService.GetString(key) ?? fallback;

    private static string FormatCoordinate(double value, bool isLatitude)
    {
        var isNegative = value < 0;
        var absolute = Math.Abs(value);
        var degrees = (int)absolute;
        var minutes = (absolute - degrees) * 60;

        var hemisphere = isLatitude
            ? isNegative ? "S" : "N"
            : isNegative ? "W" : "E";

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{hemisphere} {degrees}° {minutes:F3}'");
    }

    private void OnCompassReadingChanged(object? sender, CompassReadingChangedEventArgs e)
    {
        var magneticHeading = e.HeadingMagneticNorth;
        var trueHeading = magneticHeading;

        if (_hasDeclination)
        {
            trueHeading = magneticHeading + _declinationDegrees;
        }

        if (_smoothedHeading < 0)
        {
            _smoothedHeading = trueHeading;
        }
        else
        {
            double delta = trueHeading - _smoothedHeading;
            if (delta > 180) delta -= 360;
            if (delta < -180) delta += 360;
            _smoothedHeading += SmoothingFactor * delta;
        }

        _smoothedHeading = (_smoothedHeading % 360 + 360) % 360;

        AngleText = $"{_smoothedHeading:F0}°";
        CompassDialRotation = (360.0 - _smoothedHeading) % 360.0;
        CardinalDirection = GetCardinalDirection(_smoothedHeading);
    }

    private void OnOrientationReadingChanged(object? sender, OrientationReadingChangedEventArgs e)
    {
        double q0 = e.W;
        double q1 = e.X;
        double q2 = e.Y;
        double q3 = e.Z;

        double asinArg = 2.0 * (q0 * q2 - q3 * q1);
        if (asinArg > 1.0) asinArg = 1.0;
        if (asinArg < -1.0) asinArg = -1.0;

        double pitch = Math.Asin(asinArg);
        double roll = Math.Atan2(2.0 * (q0 * q1 + q2 * q3), 1.0 - 2.0 * (q1 * q1 + q2 * q2));

        pitch = Math.Abs(pitch * (180.0 / Math.PI));
        roll = Math.Abs(roll * (180.0 / Math.PI));

        double tiltDegrees = Math.Max(pitch, roll);

        StatusText = GetTiltStatus(tiltDegrees);
    }

    private void BuildCardinalDirections()
    {
        _cardinalDirections16 = new string[16];

        _cardinalDirections16[0] = _languageService.GetString("CompassCardinalNorthText") ?? "N";
        _cardinalDirections16[1] = $"{_languageService.GetString("CompassCardinalNorthText") ?? "N"}-{_languageService.GetString("CompassCardinalNorthEastText") ?? "NE"}";
        _cardinalDirections16[2] = _languageService.GetString("CompassCardinalNorthEastText") ?? "NE";
        _cardinalDirections16[3] = $"{_languageService.GetString("CompassCardinalEastText") ?? "E"}-{_languageService.GetString("CompassCardinalNorthEastText") ?? "NE"}";
        _cardinalDirections16[4] = _languageService.GetString("CompassCardinalEastText") ?? "E";
        _cardinalDirections16[5] = $"{_languageService.GetString("CompassCardinalEastText") ?? "E"}-{_languageService.GetString("CompassCardinalSouthEastText") ?? "SE"}";
        _cardinalDirections16[6] = _languageService.GetString("CompassCardinalSouthEastText") ?? "SE";
        _cardinalDirections16[7] = $"{_languageService.GetString("CompassCardinalSouthText") ?? "S"}-{_languageService.GetString("CompassCardinalSouthEastText") ?? "SE"}";
        _cardinalDirections16[8] = _languageService.GetString("CompassCardinalSouthText") ?? "S";
        _cardinalDirections16[9] = $"{_languageService.GetString("CompassCardinalSouthText") ?? "S"}-{_languageService.GetString("CompassCardinalSouthWestText") ?? "SW"}";
        _cardinalDirections16[10] = _languageService.GetString("CompassCardinalSouthWestText") ?? "SW";
        _cardinalDirections16[11] = $"{_languageService.GetString("CompassCardinalWestText") ?? "W"}-{_languageService.GetString("CompassCardinalSouthWestText") ?? "SW"}";
        _cardinalDirections16[12] = _languageService.GetString("CompassCardinalWestText") ?? "W";
        _cardinalDirections16[13] = $"{_languageService.GetString("CompassCardinalWestText") ?? "W"}-{_languageService.GetString("CompassCardinalNorthWestText") ?? "NW"}";
        _cardinalDirections16[14] = _languageService.GetString("CompassCardinalNorthWestText") ?? "NW";
        _cardinalDirections16[15] = $"{_languageService.GetString("CompassCardinalNorthText") ?? "N"}-{_languageService.GetString("CompassCardinalNorthWestText") ?? "NW"}";
    }

    private string GetCardinalDirection(double heading)
    {
        heading = (heading % 360 + 360) % 360;
        var dirs = _cardinalDirections16 ?? BuildCardinalDirectionsCached();
        int index = (int)Math.Round(heading / 22.5) % 16;
        return dirs[index];
    }

    private string[] BuildCardinalDirectionsCached()
    {
        BuildCardinalDirections();
        return _cardinalDirections16!;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        BuildCardinalDirections();
    }

    private void UpdateDeclination(double latitude, double longitude, double? altitudeMeters)
    {
        try
        {
#if ANDROID
            double altitudeM = altitudeMeters is > 0 ? altitudeMeters.Value : 0.0;
            var geoField = new Android.Hardware.GeomagneticField(
                (float)latitude,
                (float)longitude,
                (float)altitudeM,
                DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);

            _declinationDegrees = geoField.Declination;
            _hasDeclination = true;
            _logger.LogDebug("Compass declination updated: {Declination}° (lat={Lat}, lon={Lon}, alt={Alt})",
                _declinationDegrees.ToString("F2", CultureInfo.InvariantCulture),
                latitude.ToString("F5", CultureInfo.InvariantCulture),
                longitude.ToString("F5", CultureInfo.InvariantCulture),
                altitudeM.ToString("F1", CultureInfo.InvariantCulture));
#else
            _hasDeclination = false;
#endif
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not compute magnetic declination");
            _hasDeclination = false;
        }
    }

    private string GetTiltStatus(double tiltDegrees)
    {
        if (tiltDegrees < 10) return _languageService.GetString("CompassStatusAText") ?? string.Empty;
        if (tiltDegrees < 25) return _languageService.GetString("CompassStatusBText") ?? string.Empty;
        if (tiltDegrees < 45) return _languageService.GetString("CompassStatusCText") ?? string.Empty;
        if (tiltDegrees < 70) return _languageService.GetString("CompassStatusDText") ?? string.Empty;
        return _languageService.GetString("CompassStatusEText") ?? string.Empty;
    }

    public void Dispose()
    {
        _languageService.LanguageChanged -= OnLanguageChanged;
        _calibrationCts?.Cancel();
        _positionCts?.Cancel();
        _compassService.ReadingChanged -= OnCompassReadingChanged;
        _orientationService.ReadingChanged -= OnOrientationReadingChanged;
    }
}
