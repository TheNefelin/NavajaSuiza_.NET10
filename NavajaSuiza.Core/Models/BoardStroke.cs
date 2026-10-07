namespace NavajaSuiza.Core.Models;

public sealed class BoardStroke
{
    public string ColorHex { get; set; } = "#1F1F1F";
    public float Width { get; set; } = 4f;
    public List<BoardPoint> Points { get; } = new();

    public void AddPoint(float x, float y) => Points.Add(new BoardPoint(x, y));
}

public readonly struct BoardPoint
{
    public BoardPoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; }
    public float Y { get; }
}