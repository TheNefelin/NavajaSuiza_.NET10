using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Core.ViewModels;

public partial class DocumentReaderViewModel : BaseViewModel
{
    private const string CsvExtension = ".csv";
    private const string DocxExtension = ".docx";
    private const string XlsxExtension = ".xlsx";

    private readonly ILogger<DocumentReaderViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly IDocumentPdfConverter _documentPdfConverter;

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DetailText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ContentText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFileLoaded { get; set; }

    [ObservableProperty]
    public partial bool IsText { get; set; }

    [ObservableProperty]
    public partial bool IsCsv { get; set; }

    [ObservableProperty]
    public partial bool IsPdf { get; set; }

    [ObservableProperty]
    public partial DataTable? CsvTable { get; set; }

    [ObservableProperty]
    public partial Stream? PdfDocumentStream { get; set; }

    public DocumentReaderViewModel(
        ILogger<DocumentReaderViewModel> logger,
        ILanguageService languageService,
        IDocumentPdfConverter documentPdfConverter)
    {
        _logger = logger;
        _languageService = languageService;
        _documentPdfConverter = documentPdfConverter;
    }

    public async Task Load(string path)
    {
        try
        {
            FileName = Path.GetFileName(path);
            var extension = Path.GetExtension(path);

            if (string.Equals(extension, CsvExtension, StringComparison.OrdinalIgnoreCase))
            {
                var text = await TextFileDecoder.ReadTextAsync(path).ConfigureAwait(true);
                LoadCsv(text);
            }
            else if (string.Equals(extension, DocxExtension, StringComparison.OrdinalIgnoreCase) 
                || string.Equals(extension, XlsxExtension, StringComparison.OrdinalIgnoreCase))
            {
                var pdf = await _documentPdfConverter.ConvertToPdfAsync(path).ConfigureAwait(true);
                if (pdf is null)
                    throw new InvalidOperationException("La conversión a PDF devolvió un flujo nulo.");
                LoadPdf(pdf);
            }
            else
            {
                var text = await TextFileDecoder.ReadTextAsync(path).ConfigureAwait(true);
                LoadText(text);
            }

            IsFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir el documento {Path}", path);
            ResetState();
        }
    }

    public void Unload()
    {
        PdfDocumentStream?.Dispose();
        PdfDocumentStream = null;
    }

    private void ResetState()
    {
        FileName = string.Empty;
        DetailText = string.Empty;
        ContentText = string.Empty;
        CsvTable = null;
        IsFileLoaded = false;
        IsText = false;
        IsCsv = false;
        IsPdf = false;
        Unload();
    }

    private void LoadText(string text)
    {
        DetailText = string.Empty;
        ContentText = text;
        CsvTable = null;
        IsText = true;
        IsCsv = false;
        IsPdf = false;
    }

    private void LoadPdf(Stream pdf)
    {
        Unload();
        PdfDocumentStream = pdf;
        DetailText = string.Empty;
        ContentText = string.Empty;
        CsvTable = null;
        IsText = false;
        IsCsv = false;
        IsPdf = true;
    }

    private void LoadCsv(string text)
    {
        var rows = CsvParser.Parse(text);

        var maxColumns = rows.Count == 0 ? 0 : rows.Max(row => row.Length);
        if (maxColumns == 0)
        {
            DetailText = _languageService.GetString("DocumentReaderEmptyText");
            ContentText = string.Empty;
            CsvTable = null;
            IsText = false;
            IsCsv = false;
            IsPdf = false;
            return;
        }

        var useHeader = rows.Count >= 2;
        var header = useHeader ? rows[0] : null;

        var table = new DataTable();
        for (var i = 0; i < maxColumns; i++)
        {
            var name = header is not null && i < header.Length && !string.IsNullOrWhiteSpace(header[i])
                ? header[i]
                : $"Columna {i + 1}";
            table.Columns.Add(UniqueColumnName(table, name));
        }

        for (var r = useHeader ? 1 : 0; r < rows.Count; r++)
        {
            var row = rows[r];
            var values = new object?[maxColumns];
            for (var c = 0; c < maxColumns; c++)
                values[c] = c < row.Length ? row[c] : string.Empty;
            table.Rows.Add(values);
        }

        CsvTable = table;
        DetailText = string.Format(
            _languageService.GetString("DocumentReaderCsvRowsColumnsText"),
            table.Rows.Count,
            maxColumns);
        ContentText = string.Empty;
        IsText = false;
        IsCsv = true;
        IsPdf = false;
    }

    private static string UniqueColumnName(DataTable table, string name)
    {
        if (!table.Columns.Contains(name))
            return name;

        var suffix = 2;
        while (table.Columns.Contains($"{name} ({suffix})"))
            suffix++;
        return $"{name} ({suffix})";
    }
}
