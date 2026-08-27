using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class CompassViewModel : BaseViewModel
{
    private readonly ILogger<CompassViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly INavigationService _navigationService;
    private readonly ICompassService _compassService;
    private readonly IOrientationService _orientationService;

    [ObservableProperty]
    public partial string StatusText { get; set; } = "...";

    [ObservableProperty]
    public partial string AngleText { get; set; } = "0°";

    [ObservableProperty]
    public partial string CardinalDirection { get; set; } = "N/A";

    [ObservableProperty]
    public partial double CompassDialRotation { get; set; } = 0.0f;

    public CompassViewModel(
        ILogger<CompassViewModel> logger,
        ILanguageService languageService,
        INavigationService navigationService,
        ICompassService compassService,
        IOrientationService orientationService)
    {
        _logger = logger;
        _languageService = languageService;
        _navigationService = navigationService;
        _compassService = compassService;
        _orientationService = orientationService;
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

        _compassService.ReadingChanged += OnCompassReadingChanged;
        _compassService.Start(1, true);

        _orientationService.ReadingChanged += OnOrientationReadingChanged;
        _orientationService.Start(1);
    }

    [RelayCommand]
    public void StopSensors()
    {
        _compassService.Stop();
        _compassService.ReadingChanged -= OnCompassReadingChanged;

        _orientationService.Stop();
        _orientationService.ReadingChanged -= OnOrientationReadingChanged;
    }

    private void OnCompassReadingChanged(object? sender, CompassReadingChangedEventArgs e)
    {
        var angle = e.HeadingMagneticNorth;
        AngleText = $"{angle:F0}°";
        CompassDialRotation = 360 - angle;
        CardinalDirection = GetCardinalDirection(angle);
    }

    private void OnOrientationReadingChanged(object? sender, OrientationReadingChangedEventArgs e)
    {
        double q0 = e.W;
        double q1 = e.X;
        double q2 = e.Y;
        double q3 = e.Z;

        double pitch = Math.Asin(2 * (q0 * q2 - q3 * q1));
        double roll = Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (q1 * q1 + q2 * q2));

        pitch = Math.Abs(pitch * (180.0 / Math.PI));
        roll = Math.Abs(roll * (180.0 / Math.PI));

        double tiltDegrees = Math.Max(pitch, roll);

        StatusText = GetTiltStatus(tiltDegrees);
    }

    private string GetCardinalDirection(double heading)
    {
        heading = (heading % 360 + 360) % 360;

        string[] directions = {
            _languageService.GetString("CompassCardinalNorthText"),
            $"{_languageService.GetString("CompassCardinalNorthText")}-{_languageService.GetString("CompassCardinalNorthEastText")}",
            _languageService.GetString("CompassCardinalNorthEastText"),
            $"{_languageService.GetString("CompassCardinalEastText")}-{_languageService.GetString("CompassCardinalNorthEastText")}",
            _languageService.GetString("CompassCardinalEastText"),
            $"{_languageService.GetString("CompassCardinalEastText")}-{_languageService.GetString("CompassCardinalSouthEastText")}",
            _languageService.GetString("CompassCardinalSouthEastText"),
            $"{_languageService.GetString("CompassCardinalSouthText")}-{_languageService.GetString("CompassCardinalSouthEastText")}",
            _languageService.GetString("CompassCardinalSouthText"),
            $"{_languageService.GetString("CompassCardinalSouthText")}-{_languageService.GetString("CompassCardinalSouthWestText")}",
            _languageService.GetString("CompassCardinalSouthWestText"),
            $"{_languageService.GetString("CompassCardinalWestText")}-{_languageService.GetString("CompassCardinalSouthWestText")}",
            _languageService.GetString("CompassCardinalWestText"),
            $"{_languageService.GetString("CompassCardinalWestText")}-{_languageService.GetString("CompassCardinalNorthWestText")}",
            _languageService.GetString("CompassCardinalNorthWestText"),
            $"{_languageService.GetString("CompassCardinalNorthText")}-{_languageService.GetString("CompassCardinalNorthWestText")}"
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
