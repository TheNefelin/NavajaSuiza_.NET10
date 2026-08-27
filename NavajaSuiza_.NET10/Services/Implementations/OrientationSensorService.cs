using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class OrientationSensorService : IOrientationService
{
    public bool IsSupported => OrientationSensor.Default.IsSupported;

    public event EventHandler<OrientationReadingChangedEventArgs>? ReadingChanged;

    public void Start(double speed)
    {
        OrientationSensor.Default.ReadingChanged += OnReadingChanged;
        OrientationSensor.Default.Start((SensorSpeed)(int)speed);
    }

    public void Stop()
    {
        OrientationSensor.Default.Stop();
        OrientationSensor.Default.ReadingChanged -= OnReadingChanged;
    }

    private void OnReadingChanged(object? sender, OrientationSensorChangedEventArgs e)
    {
        var o = e.Reading.Orientation;
        ReadingChanged?.Invoke(this, new OrientationReadingChangedEventArgs(o.W, o.X, o.Y, o.Z));
    }
}
