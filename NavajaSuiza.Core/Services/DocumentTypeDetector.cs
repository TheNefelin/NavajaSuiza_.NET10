using System.IO.Compression;
using System.Text;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Services;

public static class DocumentTypeDetector
{
    private const int MaxSampleLines = 20;
    private const char CsvDelimiterComma = ',';

    private static readonly byte[] PdfSignature = { 0x25, 0x50, 0x44, 0x46, 0x2D }; // "%PDF-"
    private static readonly byte[] ZipSignature = { 0x50, 0x4B, 0x03, 0x04 };      // "PK\x03\x04"
    private static readonly byte[] OleSignature = { 0xD0, 0xCF, 0x11, 0xE0 };      // "D0 CF 11 E0"

    private const string DocxEntry = "word/document.xml";
    private const string XlsxEntry = "xl/workbook.xml";

    public static DocumentType Detect(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var type = Detect(stream);
        return type == DocumentType.Ole ? DetectOleByExtension(path) : type;
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

            if (StartsWith(stream, OleSignature))
                return DocumentType.Ole;

            return DetectTextOrCsv(stream);
        }
        finally
        {
            stream.Position = position;
        }
    }

    public static char DetectDelimiter(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return DetectDelimiter(stream);
    }

    public static char DetectDelimiter(Stream stream)
    {
        if (!stream.CanSeek)
            return CsvDelimiterComma;

        var position = stream.Position;
        try
        {
            stream.Position = 0;
            var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var lines = new List<string>();
            for (var i = 0; i < MaxSampleLines && !reader.EndOfStream; i++)
            {
                var line = reader.ReadLine();
                if (line is not null)
                    lines.Add(line);
            }

            var delimiter = GetBestCsvDelimiter(lines);
            return delimiter == '\0' ? CsvDelimiterComma : delimiter;
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

    private static DocumentType DetectOleByExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if (extension.Equals(".doc", StringComparison.OrdinalIgnoreCase))
            return DocumentType.Doc;
        if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
            return DocumentType.Xls;
        return DocumentType.Ole;
    }

    private static DocumentType DetectTextOrCsv(Stream stream)
    {
        stream.Position = 0;
        var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<string>();
        for (var i = 0; i < MaxSampleLines && !reader.EndOfStream; i++)
        {
            var line = reader.ReadLine();
            if (line is not null)
            {
                if (ContainsControlByte(line))
                    return DocumentType.Unknown;
                lines.Add(line);
            }
        }

        if (lines.Count == 0)
            return DocumentType.Unknown;

        if (LooksLikeCsv(lines))
            return DocumentType.Csv;

        return DocumentType.Text;
    }

    private static bool ContainsControlByte(string line)
    {
        foreach (var c in line)
        {
            if (char.IsControl(c) && c != '\t')
                return true;
        }

        return false;
    }

    private static bool LooksLikeCsv(List<string> lines)
    {
        return GetBestCsvDelimiter(lines) != '\0';
    }

    private static char GetBestCsvDelimiter(List<string> lines)
    {
        char[] delimiters = { ',', ';', '\t' };
        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (nonEmpty.Count < 2)
            return '\0';

        // Umbral de alineación: al menos el 90% de las líneas deben compartir el mismo conteo.
        var minAligned = (int)Math.Ceiling(nonEmpty.Count * 0.9);

        var bestDelimiter = '\0';
        var bestAligned = 0;

        foreach (var delimiter in delimiters)
        {
            var columnCounts = nonEmpty
                .Select(l => l.Count(c => c == delimiter))
                .ToList();

            if (columnCounts.Count < 2)
                continue;

            var mostCommon = columnCounts
                .GroupBy(count => count)
                .OrderByDescending(g => g.Count())
                .First();

            if (mostCommon.Key == 0 || mostCommon.Count() < minAligned)
                continue;

            if (mostCommon.Count() > bestAligned)
            {
                bestDelimiter = delimiter;
                bestAligned = mostCommon.Count();
            }
        }

        return bestDelimiter;
    }
}