using System.Text;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class TextFileDecoderTests
{
    [Fact]
    public void Decode_Utf8Bom_StripsBom()
    {
        var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes("Hola")) .ToArray();

        Assert.Equal("Hola", TextFileDecoder.Decode(bytes));
    }

    [Fact]
    public void Decode_Utf16LeBom()
    {
        var bytes = new byte[] { 0xFF, 0xFE }
            .Concat(Encoding.Unicode.GetBytes("Hola"))
            .ToArray();

        Assert.Equal("Hola", TextFileDecoder.Decode(bytes));
    }

    [Fact]
    public void Decode_Utf16BeBom()
    {
        var bytes = new byte[] { 0xFE, 0xFF }
            .Concat(Encoding.BigEndianUnicode.GetBytes("Hola"))
            .ToArray();

        Assert.Equal("Hola", TextFileDecoder.Decode(bytes));
    }

    [Fact]
    public void Decode_NoBom_ValidUtf8()
    {
        var bytes = Encoding.UTF8.GetBytes("Áéñ û");

        Assert.Equal("Áéñ û", TextFileDecoder.Decode(bytes));
    }

    [Fact]
    public void Decode_InvalidUtf8_FallsBackToLatin1()
    {
        var bytes = new byte[] { 0xE9, 0x61 }; // 'é' + 'a' en Latin-1 (no valido UTF-8)

        Assert.Equal("\u00e9a", TextFileDecoder.Decode(bytes));
    }

    [Fact]
    public async Task ReadTextAsync_ReadsFileContent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"decoder_{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllBytesAsync(path, Encoding.UTF8.GetBytes("Contenido de prueba"));
            Assert.Equal("Contenido de prueba", await TextFileDecoder.ReadTextAsync(path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Decode_EmptyBytes_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, TextFileDecoder.Decode(Array.Empty<byte>()));
        Assert.Equal(string.Empty, TextFileDecoder.Decode(null!));
    }
}