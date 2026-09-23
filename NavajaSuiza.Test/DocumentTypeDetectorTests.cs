using System.IO.Compression;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class DocumentTypeDetectorTests
{
    private static MemoryStream CreateZip(params (string Name, string Content)[] entries)
    {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                var entry = archive.CreateEntry(name);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(content);
            }
        }

        stream.Position = 0;
        return stream;
    }

    [Fact]
    public void Detect_PdfSignature_ReturnsPdf()
    {
        using var stream = new MemoryStream("%PDF-1.7"u8.ToArray());

        Assert.Equal(DocumentType.Pdf, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_ZipWithWordDocument_ReturnsDocx()
    {
        using var stream = CreateZip(("word/document.xml", "<document/>"), ("[Content_Types].xml", "<types/>"));

        Assert.Equal(DocumentType.Docx, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_ZipWithWorkbook_ReturnsXlsx()
    {
        using var stream = CreateZip(("xl/workbook.xml", "<workbook/>"), ("[Content_Types].xml", "<types/>"));

        Assert.Equal(DocumentType.Xlsx, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_ZipWithoutCanonicalEntry_ReturnsUnknown()
    {
        using var stream = CreateZip(("word/sharedStrings.xml", "<sst/>"));

        Assert.Equal(DocumentType.Unknown, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_UnknownBytes_ReturnsUnknown()
    {
        using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 });

        Assert.Equal(DocumentType.Unknown, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_EmptyStream_ReturnsUnknown()
    {
        using var stream = new MemoryStream();

        Assert.Equal(DocumentType.Unknown, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_DoesNotConsumeStreamPosition()
    {
        using var stream = new MemoryStream("%PDF-1.7"u8.ToArray());
        stream.Position = 3;

        DocumentTypeDetector.Detect(stream);

        Assert.Equal(3, stream.Position);
    }

    [Fact]
    public void Detect_Path_ReadsFromDisk()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            File.WriteAllBytes(path, "%PDF-1.7"u8.ToArray());

            Assert.Equal(DocumentType.Pdf, DocumentTypeDetector.Detect(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Detect_CsvCommaDelimited_ReturnsCsv()
    {
        using var stream = new MemoryStream("nombre,edad,ciudad\nAna,30,Madrid\nLuis,25,Bogota\n"u8.ToArray());

        Assert.Equal(DocumentType.Csv, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_CsvSemicolonDelimited_ReturnsCsv()
    {
        using var stream = new MemoryStream("nombre;edad;ciudad\nAna;30;Madrid\nLuis;25;Bogota\n"u8.ToArray());

        Assert.Equal(DocumentType.Csv, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_CsvTabDelimited_ReturnsCsv()
    {
        using var stream = new MemoryStream("nombre\tedad\tciudad\nAna\t30\tMadrid\nLuis\t25\tBogota\n"u8.ToArray());

        Assert.Equal(DocumentType.Csv, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_SingleLineCsv_ReturnsText()
    {
        using var stream = new MemoryStream("nombre,edad,ciudad"u8.ToArray());

        Assert.Equal(DocumentType.Text, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_PlainText_ReturnsText()
    {
        using var stream = new MemoryStream("Esto es un archivo de texto plano.\nSegunda linea."u8.ToArray());

        Assert.Equal(DocumentType.Text, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_CodeFile_ReturnsText()
    {
        using var stream = new MemoryStream("function sumar(a, b) {\n    return a + b;\n}\n"u8.ToArray());

        Assert.Equal(DocumentType.Text, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_OleSignature_ReturnsOle()
    {
        using var stream = new MemoryStream(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xC1, 0xD1 });

        Assert.Equal(DocumentType.Ole, DocumentTypeDetector.Detect(stream));
    }

    [Fact]
    public void Detect_OleWithDocExtension_ReturnsDoc()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".doc");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xC1, 0xD1 });

            Assert.Equal(DocumentType.Doc, DocumentTypeDetector.Detect(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Detect_OleWithXlsExtension_ReturnsXls()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xls");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xC1, 0xD1 });

            Assert.Equal(DocumentType.Xls, DocumentTypeDetector.Detect(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Detect_OleWithOtherExtension_ReturnsOle()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            File.WriteAllBytes(path, new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0xC1, 0xD1 });

            Assert.Equal(DocumentType.Ole, DocumentTypeDetector.Detect(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DetectDelimiter_Comma_ReturnsComma()
    {
        using var stream = new MemoryStream("nombre,edad,ciudad\nAna,30,Madrid\nLuis,25,Bogota\n"u8.ToArray());

        Assert.Equal(',', DocumentTypeDetector.DetectDelimiter(stream));
    }

    [Fact]
    public void DetectDelimiter_Semicolon_ReturnsSemicolon()
    {
        using var stream = new MemoryStream("nombre;edad;ciudad\nAna;30;Madrid\nLuis;25;Bogota\n"u8.ToArray());

        Assert.Equal(';', DocumentTypeDetector.DetectDelimiter(stream));
    }

    [Fact]
    public void DetectDelimiter_Tab_ReturnsTab()
    {
        using var stream = new MemoryStream("nombre\tedad\tciudad\nAna\t30\tMadrid\nLuis\t25\tBogota\n"u8.ToArray());

        Assert.Equal('\t', DocumentTypeDetector.DetectDelimiter(stream));
    }

    [Fact]
    public void DetectDelimiter_NoDelimiter_ReturnsCommaDefault()
    {
        using var stream = new MemoryStream("solo texto\nsin delimitadores consistentes\n"u8.ToArray());

        Assert.Equal(',', DocumentTypeDetector.DetectDelimiter(stream));
    }
}