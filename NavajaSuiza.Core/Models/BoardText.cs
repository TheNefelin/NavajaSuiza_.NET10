namespace NavajaSuiza.Core.Models;

public sealed class BoardText
{
    public string Content { get; set; } = string.Empty;

    public float X { get; set; }

    public float Y { get; set; }

    public float FontSize { get; set; } = BoardDefaults.FontSize;

    public string ColorHex { get; set; } = "#1F1F1F";
}