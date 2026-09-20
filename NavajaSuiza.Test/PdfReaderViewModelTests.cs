using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class PdfReaderViewModelTests
{
    private const string TestString = "Test";

    private readonly Mock<ILogger<PdfReaderViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IFilePickerService> _filePickerServiceMock = new();

    private PdfReaderViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns(TestString);
        return new PdfReaderViewModel(
            _loggerMock.Object,
            _languageServiceMock.Object,
            _filePickerServiceMock.Object);
    }

    private static async Task<string> CreateTempPdfFileAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"navaja-test-{Guid.NewGuid():N}.pdf");
        await File.WriteAllBytesAsync(path, new byte[] { 0x25, 0x50, 0x44, 0x46 });
        return path;
    }

    [Fact]
    public async Task OpenDocument_LoadsStreamAndFileName()
    {
        var path = await CreateTempPdfFileAsync();
        PdfReaderViewModel vm = null!;
        try
        {
            _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>())).ReturnsAsync(path);

            vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.Equal(Path.GetFileName(path), vm.FileName);
            Assert.NotNull(vm.PdfDocumentStream);
            Assert.Equal(string.Empty, vm.HintText);
        }
        finally
        {
            if (vm is not null)
                vm.Unload();
            File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_WhenCancelled_KeepsState()
    {
        _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

        var vm = CreateSut();
        await vm.OpenDocumentCommand.ExecuteAsync(null);

        Assert.False(vm.IsFileLoaded);
        Assert.Equal(string.Empty, vm.FileName);
        Assert.Null(vm.PdfDocumentStream);
        Assert.Equal(TestString, vm.HintText);
    }

    [Fact]
    public async Task OpenDocument_WhenFileNotReadable_SetsErrorHint()
    {
        _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>()))
            .ReturnsAsync("ruta-inexistente.pdf");

        var vm = CreateSut();
        await vm.OpenDocumentCommand.ExecuteAsync(null);

        Assert.False(vm.IsFileLoaded);
        Assert.Equal(string.Empty, vm.FileName);
        Assert.Null(vm.PdfDocumentStream);
        Assert.Equal(TestString, vm.HintText);
    }

    [Fact]
    public async Task Unload_DisposesStreamAndResetsState()
    {
        var path = await CreateTempPdfFileAsync();
        try
        {
            _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>())).ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);
            Assert.NotNull(vm.PdfDocumentStream);

            vm.Unload();

            Assert.False(vm.IsFileLoaded);
            Assert.Equal(string.Empty, vm.FileName);
            Assert.Null(vm.PdfDocumentStream);
            Assert.Equal(TestString, vm.HintText);
        }
        finally
        {
            File.Delete(path);
        }
    }
}