using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class CompassViewModel : ObservableObject
{
    private readonly ILogger<TestingViewModel> _logger;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private string _statusText = "...";

    [ObservableProperty]
    private string _angleText = "0°";

    [ObservableProperty]
    private string _cardinalDirection = "N/A";

    [ObservableProperty]
    private double _CompassDialRotation = 0.0f;

    public CompassViewModel(
        ILogger<TestingViewModel> logger,
        ILanguageService languageService)
    {
        _logger = logger;
        _languageService = languageService;

        ValidateSensors();
    }

    private async void ValidateSensors()
    {
        if (!Compass.Default.IsSupported || !OrientationSensor.Default.IsSupported)
        {
            _logger.LogWarning("Navigating back due to unsupported sensors");

            await Shell.Current.DisplayAlertAsync("Error", "Los sensores necesarios no son soportados en este dispositivo.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    public void StartSensorsAsync()
    {
        Compass.Default.ReadingChanged += OnCompassReadingChanged;
        Compass.Default.Start(SensorSpeed.UI, applyLowPassFilter: true);

        OrientationSensor.Default.ReadingChanged += OnOrientationReadingChanged;
        OrientationSensor.Default.Start(SensorSpeed.UI);
    }

    [RelayCommand]
    public void StopSensors()
    {
        Compass.Default.Stop();
        Compass.Default.ReadingChanged -= OnCompassReadingChanged;

        OrientationSensor.Default.Stop();
        OrientationSensor.Default.ReadingChanged -= OnOrientationReadingChanged;
    }

    private void OnCompassReadingChanged(object sender, CompassChangedEventArgs e)
    {
        var headingMagneticNorth = e.Reading.HeadingMagneticNorth;

        var angle = headingMagneticNorth;
        AngleText = $"{angle:F0}°";
        CompassDialRotation = 360 - angle;
        CardinalDirection = GetCardinalDirection(angle);
    }

    private void OnOrientationReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        var reading = e.Reading;

        // Método PRECISO usando Quaternion
        double q0 = reading.Orientation.W;
        double q1 = reading.Orientation.X;
        double q2 = reading.Orientation.Y;
        double q3 = reading.Orientation.Z;

        // Calcular pitch (inclinación frontal) y roll (inclinación lateral)
        double pitch = Math.Asin(2 * (q0 * q2 - q3 * q1));
        double roll = Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (q1 * q1 + q2 * q2));

        // Convertir a grados y tomar valor absoluto
        pitch = Math.Abs(pitch * (180.0 / Math.PI));
        roll = Math.Abs(roll * (180.0 / Math.PI));

        // La inclinación total es el máximo de pitch y roll
        double tiltDegrees = Math.Max(pitch, roll);

        StatusText = GetTiltStatus(tiltDegrees);
    }

    private string GetCardinalDirection(double heading)
    {
        heading = (heading % 360 + 360) % 360;

        string[] directions = {
            _languageService.GetString("CompassCardinalNorthText"), //N
            $"{_languageService.GetString("CompassCardinalNorthText")}-{_languageService.GetString("CompassCardinalNorthEastText")}", //N-NE
            _languageService.GetString("CompassCardinalNorthEastText"), //NE
            $"{_languageService.GetString("CompassCardinalEastText")}-{_languageService.GetString("CompassCardinalNorthEastText")}", //E-NE
            _languageService.GetString("CompassCardinalEastText"), //E
            $"{_languageService.GetString("CompassCardinalEastText")}-{_languageService.GetString("CompassCardinalSouthEastText")}", //E-SE
            _languageService.GetString("CompassCardinalSouthEastText"), //SE
            $"{_languageService.GetString("CompassCardinalSouthText")}-{_languageService.GetString("CompassCardinalSouthEastText")}", //S-SE
            _languageService.GetString("CompassCardinalSouthText"), //S
            $"{_languageService.GetString("CompassCardinalSouthText")}-{_languageService.GetString("CompassCardinalSouthWestText")}", //S-SW
            _languageService.GetString("CompassCardinalSouthWestText"), //SW
            $"{_languageService.GetString("CompassCardinalWestText")}-{_languageService.GetString("CompassCardinalSouthWestText")}", //W-SW
            _languageService.GetString("CompassCardinalWestText"), //W
            $"{_languageService.GetString("CompassCardinalWestText")}-{_languageService.GetString("CompassCardinalNorthWestText")}", //W-NW
            _languageService.GetString("CompassCardinalNorthWestText"), //NW
            $"{_languageService.GetString("CompassCardinalNorthText")}-{_languageService.GetString("CompassCardinalNorthWestText")}" //N-NW
        };

        int index = (int)Math.Round(heading / 22.5) % 16;
        return directions[index];
    }

    private string GetTiltStatus(double tiltDegrees)
    {        
        if (tiltDegrees < 10) return _languageService.GetString("CompassStatusAText");
        if (tiltDegrees < 25) return _languageService.GetString("CompassStatusBText");
        if (tiltDegrees < 45) return _languageService.GetString("CompassStatusCText");
        if (tiltDegrees < 70) return _languageService.GetString("CompassStatusDText");
        return _languageService.GetString("CompassStatusEText");
    }
}