namespace NavajaSuiza.Core.Models;

public sealed class PizarraStroke
{
    public string ColorHex { get; set; } = "#1F1F1F";
    public float Width { get; set; } = 4f;
    public List<PizarraPoint> Points { get; } = new();

    public void AddPoint(float x, float y) => Points.Add(new PizarraPoint(x, y));
}

public readonly struct PizarraPoint
{
    public PizarraPoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; }
    public float Y { get; }
}