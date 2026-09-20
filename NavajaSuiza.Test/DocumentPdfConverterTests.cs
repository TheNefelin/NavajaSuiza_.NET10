using NavajaSuiza.Core.Services;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;

namespace NavajaSuiza.Test;

public class DocumentPdfConverterTests
{
    private readonly DocumentPdfConverter _converter = new();

    [Fact]
    public async Task ConvertToPdfAsync_Docx_ReturnsPdfStream()
    {
        var path = Path.Combine(Path.GetTempPath(), $"doc_{Guid.NewGuid():N}.docx");

        try
        {
            CreateSampleDocx(path);

            var result = await _converter.ConvertToPdfAsync(path);

            Assert.NotNull(result);
            Assert.True(result!.Length > 0);
            Assert.Equal(0, result.Position);
            result.Dispose();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task ConvertToPdfAsync_Xlsx_ReturnsPdfStream()
    {
        var path = Path.Combine(Path.GetTempPath(), $"xls_{Guid.NewGuid():N}.xlsx");

        try
        {
            CreateSampleXlsx(path);

            var result = await _converter.ConvertToPdfAsync(path);

            Assert.NotNull(result);
            Assert.True(result!.Length > 0);
            Assert.Equal(0, result.Position);
            result.Dispose();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task ConvertToPdfAsync_UnsupportedExtension_ReturnsNull()
    {
        var path = Path.Combine(Path.GetTempPath(), $"plain_{Guid.NewGuid():N}.txt");

        var result = await _converter.ConvertToPdfAsync(path);

        Assert.Null(result);
    }

    [Fact]
    public async Task ConvertToPdfAsync_MissingFile_Throws()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.docx");

        await Assert.ThrowsAnyAsync<Exception>(() => _converter.ConvertToPdfAsync(missingPath));
    }

    private static void CreateSampleDocx(string path)
    {
        using var document = new WordDocument();
        document.EnsureMinimal();
        document.LastParagraph.AppendText("Hola mundo desde el lector");

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        document.Save(stream, FormatType.Docx);
    }

    private static void CreateSampleXlsx(string path)
    {
        using var engine = new ExcelEngine();
        var application = engine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        var workbook = application.Workbooks.Create();
        var sheet = workbook.Worksheets[0];
        sheet.Range["A1"].Value = "Nombre";
        sheet.Range["B1"].Value = "Edad";
        sheet.Range["A2"].Value = "Ana";
        sheet.Range["B2"].Number = 30;

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        workbook.SaveAs(stream);
        workbook.Close();
    }
}