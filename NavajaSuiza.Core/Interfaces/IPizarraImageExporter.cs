using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public enum PizarraExportResult
{
    Saved,
    NotAvailable,
    Failed
}

public interface IPizarraImageExporter
{
    Task<PizarraExportResult> ExportAsync(IReadOnlyList<PizarraStroke> strokes, string boardColorHex);
}