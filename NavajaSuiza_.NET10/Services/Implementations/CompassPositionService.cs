using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class CompassPositionService : ICompassPositionService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);

    public bool IsAvailable => Geolocation.Default.IsEnabled;

    public async Task<PositionReading> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
    {
        var permission = await new Permissions.LocationWhenInUse().RequestAsync();

        if (permission != PermissionStatus.Granted)
            throw new UnauthorizedAccessException("Location permission was not granted.");

        var request = new GeolocationRequest(GeolocationAccuracy.High, RequestTimeout)
        {
            RequestFullAccuracy = true
        };

        var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken)
            ?? throw new InvalidOperationException("The device did not return a position.");

        return new PositionReading(
            location.Latitude,
            location.Longitude,
            location.Altitude > 0 ? location.Altitude : null,
            location.Accuracy);
    }
}