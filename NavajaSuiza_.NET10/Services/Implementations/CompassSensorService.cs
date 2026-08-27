using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class CompassSensorService : ICompassService
{
    public bool IsSupported => Compass.Default.IsSupported;

    public event EventHandler<CompassReadingChangedEventArgs>? ReadingChanged;

    public void Start(double speed, bool applyLowPassFilter)
    {
        Compass.Default.ReadingChanged += OnReadingChanged;
        Compass.Default.Start(SensorSpeed.UI, applyLowPassFilter);
    }

    public void Stop()
    {
        Compass.Default.Stop();
        Compass.Default.ReadingChanged -= OnReadingChanged;
    }

    private void OnReadingChanged(object? sender, CompassChangedEventArgs e)
    {
        ReadingChanged?.Invoke(this, new CompassReadingChangedEventArgs(e.Reading.HeadingMagneticNorth));
    }
}
