using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public enum BoardExportResult
{
    Saved,
    NotAvailable,
    Failed
}

public interface IBoardImageExporter
{
    Task<BoardExportResult> ExportAsync(
        IReadOnlyList<BoardStroke> strokes,
        IReadOnlyList<BoardText> texts,
        string boardColorHex);
}