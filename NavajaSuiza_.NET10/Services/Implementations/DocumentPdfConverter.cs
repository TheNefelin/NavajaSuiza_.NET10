using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class DocumentPdfConverter : IDocumentPdfConverter
{
    private readonly ILogger<DocumentPdfConverter> _logger;

    public DocumentPdfConverter(ILogger<DocumentPdfConverter> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> ConvertToPdfAsync(DocumentType documentType, string path)
    {
        return await Task.Run(() => ConvertToPdf(documentType, path)).ConfigureAwait(false);
    }

    private MemoryStream ConvertToPdf(DocumentType documentType, string path)
    {
        using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        _logger.LogInformation("Convirtiendo {Type} a PDF: {Path}", documentType, path);

        try
        {
            var output = new MemoryStream();
            switch (documentType)
            {
                case DocumentType.Docx:
                    ConvertWordToPdf(input, output);
                    break;
                case DocumentType.Xlsx:
                    ConvertExcelToPdf(input, output);
                    break;
                case DocumentType.Csv:
                    ConvertCsvToPdf(path, output);
                    break;
                default:
                    throw new NotSupportedException($"Tipo de documento no convertible: {documentType}");
            }

            output.Position = 0;
            return output;
        }
        catch
        {
            _logger.LogError("Error al convertir {Type} a PDF: {Path}", documentType, path);
            throw;
        }
    }

    private static void ConvertWordToPdf(Stream input, MemoryStream output)
    {
        using var document = new WordDocument(input, FormatType.Docx);
        using var renderer = new DocIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(document);
        pdfDocument.Save(output);
    }

    private static void ConvertExcelToPdf(Stream input, MemoryStream output)
    {
        using var engine = new ExcelEngine();
        var application = engine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;
        var workbook = application.Workbooks.Open(input);
        using var renderer = new XlsIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(workbook);
        pdfDocument.Save(output);
    }

    private static void ConvertCsvToPdf(string path, MemoryStream output)
    {
        var delimiter = DocumentTypeDetector.DetectDelimiter(path);

        using var engine = new ExcelEngine();
        var application = engine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;
        var workbook = application.Workbooks.Open(path, delimiter.ToString());
        using var renderer = new XlsIORenderer();
        using var pdfDocument = renderer.ConvertToPDF(workbook);
        pdfDocument.Save(output);
    }
}