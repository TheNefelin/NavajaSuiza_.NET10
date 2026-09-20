using System.Text;

namespace NavajaSuiza.Core.Services;

public static class TextFileDecoder
{
    private static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };
    private static readonly byte[] Utf16LeBom = { 0xFF, 0xFE };
    private static readonly byte[] Utf16BeBom = { 0xFE, 0xFF };

    public static async Task<string> ReadTextAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Decode(bytes);
    }

    public static string Decode(byte[] bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return string.Empty;

        if (HasBom(bytes, Utf8Bom))
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetString(bytes, 3, bytes.Length - 3);

        if (HasBom(bytes, Utf16LeBom))
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

        if (HasBom(bytes, Utf16BeBom))
            return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

        try
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private static bool HasBom(byte[] bytes, byte[] bom)
    {
        if (bytes.Length < bom.Length)
            return false;

        for (var i = 0; i < bom.Length; i++)
        {
            if (bytes[i] != bom[i])
                return false;
        }

        return true;
    }
}