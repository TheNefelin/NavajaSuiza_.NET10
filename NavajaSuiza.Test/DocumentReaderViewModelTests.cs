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
    private readonly Mock<IDocumentPdfConverter> _documentPdfConverterMock = new();

    private DocumentReaderViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns(TestString);
        return new DocumentReaderViewModel(
            _loggerMock.Object,
            _languageServiceMock.Object,
            _filePickerServiceMock.Object,
            _documentPdfConverterMock.Object);
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
            Assert.True(vm.IsText);
            Assert.False(vm.IsCsv);
            Assert.False(vm.IsPdf);
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
    public async Task OpenDocument_Csv_BuildsDataTableWithHeader()
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
            Assert.True(vm.IsCsv);
            Assert.False(vm.IsText);
            Assert.False(vm.IsPdf);
            Assert.Equal(Path.GetFileName(path), vm.FileName);
            Assert.Equal(string.Empty, vm.ContentText);

            Assert.NotNull(vm.CsvTable);
            Assert.Equal(2, vm.CsvTable!.Rows.Count);
            Assert.Equal(new[] { "nombre", "edad" }, vm.CsvTable.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName));
            Assert.Equal("Ana", vm.CsvTable.Rows[0][0]);
            Assert.Equal("30", vm.CsvTable.Rows[0][1]);
            Assert.Equal("Luis", vm.CsvTable.Rows[1][0]);
            Assert.Equal("41", vm.CsvTable.Rows[1][1]);
            Assert.Equal(TestString, vm.DetailText);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_Csv_RaggedRows_UsesGenericColumnsAndPads()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.csv");
        await File.WriteAllTextAsync(path, "Nombre,Edad\nAna\nLuis,41,Extra\n");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.True(vm.IsCsv);
            Assert.NotNull(vm.CsvTable);

            Assert.Equal(new[] { "Nombre", "Edad", "Columna 3" },
                vm.CsvTable!.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName));
            Assert.Equal(2, vm.CsvTable.Rows.Count);
            Assert.Equal("Ana", vm.CsvTable.Rows[0][0]);
            Assert.Equal(string.Empty, vm.CsvTable.Rows[0][1]);
            Assert.Equal(string.Empty, vm.CsvTable.Rows[0][2]);
            Assert.Equal("Luis", vm.CsvTable.Rows[1][0]);
            Assert.Equal("41", vm.CsvTable.Rows[1][1]);
            Assert.Equal("Extra", vm.CsvTable.Rows[1][2]);
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
            Assert.False(vm.IsCsv);
            Assert.Null(vm.CsvTable);
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
    public async Task OpenDocument_Docx_ConvertsToPdfStream()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.docx");
        var pdfStream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(path, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pdfStream);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.True(vm.IsFileLoaded);
            Assert.True(vm.IsPdf);
            Assert.False(vm.IsText);
            Assert.False(vm.IsCsv);
            Assert.Same(pdfStream, vm.PdfDocumentStream);
            Assert.Equal(string.Empty, vm.ContentText);
        }
        finally
        {
            pdfStream.Dispose();
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public async Task OpenDocument_DocxWhenConverterReturnsNull_SetsErrorHint()
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}.docx");

        try
        {
            _filePickerServiceMock
                .Setup(s => s.PickDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(path);
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(path, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MemoryStream?)null);

            var vm = CreateSut();
            await vm.OpenDocumentCommand.ExecuteAsync(null);

            Assert.False(vm.IsFileLoaded);
            Assert.False(vm.IsPdf);
            Assert.Equal(string.Empty, vm.FileName);
            Assert.Equal(TestString, vm.HintText);
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