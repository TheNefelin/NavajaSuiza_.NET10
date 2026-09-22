namespace NavajaSuiza.Core.Models;

public sealed class PdfReaderPayload
{
    public string? Path { get; init; }
    public Stream? Stream { get; init; }
    public string FileName { get; init; } = string.Empty;
}