namespace NavajaSuiza.Core.Interfaces;

public interface ICompassService
{
    bool IsSupported { get; }
    event EventHandler<CompassReadingChangedEventArgs>? ReadingChanged;
    void Start(double speed, bool applyLowPassFilter);
    void Stop();
}

public class CompassReadingChangedEventArgs(double headingMagneticNorth) : EventArgs
{
    public double HeadingMagneticNorth { get; } = headingMagneticNorth;
}
