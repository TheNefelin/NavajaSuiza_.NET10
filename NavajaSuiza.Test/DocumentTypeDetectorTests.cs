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
}