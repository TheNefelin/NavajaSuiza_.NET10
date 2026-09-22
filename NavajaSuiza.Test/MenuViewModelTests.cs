using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class MenuViewModelTests
{
    private const string TestString = "Test";
    private const string PdfBytes = "%PDF-";

    private readonly Mock<ILogger<MenuViewModel>> _loggerMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<IDeviceStatusService> _deviceStatusServiceMock = new();
    private readonly Mock<IFilePickerService> _filePickerServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IDocumentPdfConverter> _documentPdfConverterMock = new();

    private MenuViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns(TestString);
        return new MenuViewModel(
            _loggerMock.Object,
            _navigationServiceMock.Object,
            _deviceStatusServiceMock.Object,
            _filePickerServiceMock.Object,
            _languageServiceMock.Object,
            _documentPdfConverterMock.Object);
    }

    private static string CreateTempFile(string baseName, byte[] content)
    {
        var name = Guid.NewGuid().ToString("N") + baseName;
        var path = Path.Combine(Path.GetTempPath(), name);
        File.WriteAllBytes(path, content);
        return path;
    }

    private static string CreateTempDocxPath()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".docx");
        using var file = File.Create(path);
        using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("word/document.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("<document/>");
        }

        return path;
    }

    [Fact]
    public void BatteryLevel_DefaultsToZero()
    {
        var vm = CreateSut();
        Assert.Equal("0%", vm.BatteryLevel);
    }

    [Fact]
    public void AvailableStorage_DefaultsToZeroGB()
    {
        var vm = CreateSut();
        Assert.Equal("0 GB", vm.AvailableStorage);
    }

    [Fact]
    public void OnPageAppearing_UpdatesBatteryAndStorage()
    {
        _deviceStatusServiceMock.Setup(s => s.GetBatteryLevel()).Returns("85%");
        _deviceStatusServiceMock.Setup(s => s.GetAvailableStorage()).Returns("32 GB");

        var vm = CreateSut();
        vm.OnPageAppearing();

        Assert.Equal("85%", vm.BatteryLevel);
        Assert.Equal("32 GB", vm.AvailableStorage);
    }

    [Fact]
    public void NavigateToFlashlight_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToFlashlightCommand.Execute(null);

        _navigationServiceMock.Verify(s => s.PushAsync("FlashlightPage"), Times.Once);
    }

    [Fact]
    public async Task NavigateToPdfReader_PdfNavigatesWithPath()
    {
        var path = CreateTempFile(".pdf", System.Text.Encoding.ASCII.GetBytes(PdfBytes));
        try
        {
            _filePickerServiceMock.Setup(s => s.PickDocumentAsync(It.IsAny<string>())).ReturnsAsync(path);

            var vm = CreateSut();
            await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

            _filePickerServiceMock.Verify(s => s.PickDocumentAsync(TestString), Times.Once);
            _navigationServiceMock.Verify(
                s => s.PushAsync("PdfReaderPage", It.Is<PdfReaderPayload>(p => p.Path == path && ReferenceEquals(p.Stream, null))),
                Times.Once);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task NavigateToPdfReader_DocxConvertsAndNavigatesWithStream()
    {
        var path = CreateTempDocxPath();
        using var converted = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D });
        try
        {
            _filePickerServiceMock.Setup(s => s.PickDocumentAsync(It.IsAny<string>())).ReturnsAsync(path);
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(DocumentType.Docx, path))
                .ReturnsAsync(converted);

            var vm = CreateSut();
            await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

            _documentPdfConverterMock.Verify(s => s.ConvertToPdfAsync(DocumentType.Docx, path), Times.Once);
            _navigationServiceMock.Verify(
                s => s.PushAsync("PdfReaderPage", It.Is<PdfReaderPayload>(p => p.Stream == converted)),
                Times.Once);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task NavigateToPdfReader_Docx_SetsIsConvertingWhileConverting()
    {
        var path = CreateTempDocxPath();
        var tcs = new TaskCompletionSource<Stream>();
        try
        {
            _filePickerServiceMock.Setup(s => s.PickDocumentAsync(It.IsAny<string>())).ReturnsAsync(path);
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(DocumentType.Docx, path))
                .Returns(tcs.Task);

            var vm = CreateSut();
            var commandTask = vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

            Assert.True(vm.IsConverting);

            tcs.SetResult(new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }));
            await commandTask;

            Assert.False(vm.IsConverting);
            _navigationServiceMock.Verify(s => s.PushAsync("PdfReaderPage", It.IsAny<PdfReaderPayload>()), Times.Once);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task NavigateToPdfReader_Docx_WhenConversionFails_DoesNotNavigate()
    {
        var path = CreateTempDocxPath();
        try
        {
            _filePickerServiceMock.Setup(s => s.PickDocumentAsync(It.IsAny<string>())).ReturnsAsync(path);
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(DocumentType.Docx, path))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var vm = CreateSut();
            await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

            Assert.False(vm.IsConverting);
            _navigationServiceMock.Verify(s => s.PushAsync(It.IsAny<string>(), It.IsAny<PdfReaderPayload>()), Times.Never);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task NavigateToPdfReader_WhenPickerCancelled_DoesNotNavigate()
    {
        _filePickerServiceMock.Setup(s => s.PickDocumentAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

        var vm = CreateSut();
        await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

        _filePickerServiceMock.Verify(s => s.PickDocumentAsync(TestString), Times.Once);
        _navigationServiceMock.Verify(s => s.PushAsync(It.IsAny<string>(), It.IsAny<PdfReaderPayload>()), Times.Never);
    }
}