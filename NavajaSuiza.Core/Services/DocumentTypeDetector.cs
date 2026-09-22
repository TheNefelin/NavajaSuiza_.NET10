using System.IO.Compression;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Services;

public static class DocumentTypeDetector
{
    private static readonly byte[] PdfSignature = { 0x25, 0x50, 0x44, 0x46, 0x2D }; // "%PDF-"
    private static readonly byte[] ZipSignature = { 0x50, 0x4B, 0x03, 0x04 };      // "PK\x03\x04"

    private const string DocxEntry = "word/document.xml";
    private const string XlsxEntry = "xl/workbook.xml";

    public static DocumentType Detect(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Detect(stream);
    }

    public static DocumentType Detect(Stream stream)
    {
        if (!stream.CanSeek)
            return DocumentType.Unknown;

        var position = stream.Position;
        try
        {
            if (StartsWith(stream, PdfSignature))
                return DocumentType.Pdf;

            if (StartsWith(stream, ZipSignature))
                return DetectOoxml(stream);

            return DocumentType.Unknown;
        }
        finally
        {
            stream.Position = position;
        }
    }

    private static bool StartsWith(Stream stream, byte[] signature)
    {
        var position = stream.Position;
        var buffer = new byte[signature.Length];
        var read = stream.Read(buffer, 0, signature.Length);
        stream.Position = position;

        if (read < signature.Length)
            return false;

        for (var i = 0; i < signature.Length; i++)
        {
            if (buffer[i] != signature[i])
                return false;
        }

        return true;
    }

    private static DocumentType DetectOoxml(Stream stream)
    {
        stream.Position = 0;
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in archive.Entries)
        {
            if (string.Equals(entry.FullName, DocxEntry, StringComparison.Ordinal))
                return DocumentType.Docx;
            if (string.Equals(entry.FullName, XlsxEntry, StringComparison.Ordinal))
                return DocumentType.Xlsx;
        }

        return DocumentType.Unknown;
    }
}