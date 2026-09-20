using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class DocumentReaderViewModelTests
{
    private const string TestString = "Test";

    private readonly Mock<ILogger<DocumentReaderViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IFilePickerService> _filePickerServiceMock = new();

    private DocumentReaderViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns(TestString);
        return new DocumentReaderViewModel(_loggerMock.Object, _languageServiceMock.Object, _filePickerServiceMock.Object);
    }

    [Fact]
    public async Task OpenDocument_ReadsTxtContent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "Un texto simple");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.Equal(Path.GetFileName(path), vm.FileName);
            Assert.Equal("Un texto simple", vm.ContentText);
            Assert.Equal(string.Empty, vm.DetailText);
            Assert.Equal(string.Empty, vm.HintText);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_Csv_ParsesRowsAndColumns()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, "nombre,edad\nAna,30\nLuis,41\n");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.Equal(Path.GetFileName(path), vm.FileName);
            Assert.Equal($"nombre | edad{Environment.NewLine}Ana | 30{Environment.NewLine}Luis | 41", vm.ContentText);
            Assert.Equal(TestString, vm.DetailText);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_EmptyCsv_SetsEmptyMessage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, "\n\n");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.Equal(string.Empty, vm.ContentText);
            Assert.Equal(TestString, vm.DetailText);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_WhenCancelled_KeepsState()
    {
        _filePickerServiceMock
            .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var vm = CreateSut();
        await vm.OpenDocumentCommand.ExecuteAsync(null);

        Assert.False(vm.IsFileLoaded);
        Assert.Equal(string.Empty, vm.FileName);
        Assert.Equal(string.Empty, vm.ContentText);
        Assert.Equal(TestString, vm.HintText);
    }

    [Fact]
    public async Task OpenDocument_MissingFile_SetsErrorHint()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"reader_missing_{Guid.NewGuid():N}.txt");

        _filePickerServiceMock
            .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(missingPath);

        var vm = CreateSut();
        await vm.OpenDocumentCommand.ExecuteAsync(null);

        Assert.False(vm.IsFileLoaded);
        Assert.Equal(string.Empty, vm.FileName);
        Assert.Equal(string.Empty, vm.ContentText);
        Assert.Equal(TestString, vm.HintText);
    }

    [Fact]
    public async Task OpenDocument_AsksPickerWithLocalizedTitle()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "x");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            _filePickerServiceMock.Verify(s => s.PickDocumentAsync(TestString), Times.Once);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}