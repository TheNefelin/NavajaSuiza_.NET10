namespace NavajaSuiza.Core.Interfaces;

public interface IOrientationService
{
    bool IsSupported { get; }
    event EventHandler<OrientationReadingChangedEventArgs>? ReadingChanged;
    void Start(double speed);
    void Stop();
}

public class OrientationReadingChangedEventArgs(double w, double x, double y, double z) : EventArgs
{
    public double W { get; } = w;
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Z { get; } = z;
}
