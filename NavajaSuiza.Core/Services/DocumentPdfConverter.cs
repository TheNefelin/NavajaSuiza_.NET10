using NavajaSuiza.Core.Interfaces;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

namespace NavajaSuiza.Core.Services;

public class DocumentPdfConverter : IDocumentPdfConverter
{
    public Task<MemoryStream?> ConvertToPdfAsync(string path, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path);
        return extension.ToLowerInvariant() switch
        {
            ".docx" => ConvertWordToPdfAsync(path, cancellationToken),
            ".xlsx" => ConvertExcelToPdfAsync(path, cancellationToken),
            _ => Task.FromResult<MemoryStream?>(null),
        };
    }

    private static Task<MemoryStream?> ConvertWordToPdfAsync(string path, CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var documentStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var wordDocument = new WordDocument(documentStream, FormatType.Automatic);
                using var renderer = new DocIORenderer();
                using var pdfDocument = renderer.ConvertToPDF(wordDocument);

                return SaveToStream(pdfDocument);
            },
            cancellationToken);
    }

    private static Task<MemoryStream?> ConvertExcelToPdfAsync(string path, CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var excelEngine = new ExcelEngine();
                var workbook = excelEngine.Excel.Workbooks.Open(stream);
                using var renderer = new XlsIORenderer();
                using var pdfDocument = renderer.ConvertToPDF(workbook);

                return SaveToStream(pdfDocument);
            },
            cancellationToken);
    }

    private static MemoryStream SaveToStream(PdfDocument pdfDocument)
    {
        MemoryStream? output = null;
        try
        {
            output = new MemoryStream();
            pdfDocument.Save(output);
            output.Position = 0;
            return output;
        }
        catch
        {
            output?.Dispose();
            throw;
        }
    }
}