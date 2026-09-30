namespace NavajaSuiza.Core.Models;

public class PositionReading(
    double latitude,
    double longitude,
    double? altitudeMeters,
    double? horizontalAccuracyMeters)
{
    public double Latitude { get; } = latitude;

    public double Longitude { get; } = longitude;

    public double? AltitudeMeters { get; } = altitudeMeters;

    public double? HorizontalAccuracyMeters { get; } = horizontalAccuracyMeters;
}