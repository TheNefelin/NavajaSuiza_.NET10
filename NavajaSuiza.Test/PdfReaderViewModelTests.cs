using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class PdfReaderViewModelTests
{
    private readonly Mock<ILogger<PdfReaderViewModel>> _loggerMock = new();

    private PdfReaderViewModel CreateSut() => new(_loggerMock.Object);

    private static string CreateTempPdfPath()
    {
        var name = Guid.NewGuid().ToString("N") + ".pdf";
        var path = Path.Combine(Path.GetTempPath(), name);
        File.WriteAllBytes(path, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        return path;
    }

    [Fact]
    public void Load_ValidPath_LoadsStreamAndFileName()
    {
        var path = CreateTempPdfPath();
        var sut = CreateSut();
        try
        {
            sut.Load(path);

            Assert.True(sut.IsFileLoaded);
            Assert.Equal(Path.GetFileName(path), sut.FileName);
            Assert.NotNull(sut.PdfDocumentStream);
        }
        finally
        {
            sut.Unload();
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_InvalidPath_ResetsState()
    {
        var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        var sut = CreateSut();

        sut.Load(missing);

        Assert.False(sut.IsFileLoaded);
        Assert.Equal(string.Empty, sut.FileName);
        Assert.Null(sut.PdfDocumentStream);
    }

    [Fact]
    public void Unload_DisposesStreamAndResetsState()
    {
        var path = CreateTempPdfPath();
        var sut = CreateSut();
        try
        {
            sut.Load(path);
            Assert.True(sut.IsFileLoaded);
        }
        finally
        {
            sut.Unload();
            File.Delete(path);
        }

        Assert.False(sut.IsFileLoaded);
        Assert.Equal(string.Empty, sut.FileName);
        Assert.Null(sut.PdfDocumentStream);
    }
}
