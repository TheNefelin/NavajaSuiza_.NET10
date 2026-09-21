using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class DocumentReaderViewModelTests
{
    private readonly Mock<ILogger<DocumentReaderViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IDocumentPdfConverter> _documentPdfConverterMock = new();

    private DocumentReaderViewModel CreateSut() => new(
        _loggerMock.Object,
        _languageServiceMock.Object,
        _documentPdfConverterMock.Object);

    private static string CreateTempFile(string extension, string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"reader_{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task Load_Txt_SetsFileLoadedAndTextState()
    {
        var path = CreateTempFile(".txt", "Contenido de prueba");

        try
        {
            var vm = CreateSut();
            await vm.Load(path);

            Assert.True(vm.IsFileLoaded);
            Assert.True(vm.IsText);
            Assert.False(vm.IsCsv);
            Assert.False(vm.IsPdf);
            Assert.Equal(Path.GetFileName(path), vm.FileName);
            Assert.Equal("Contenido de prueba", vm.ContentText);
            Assert.Equal(string.Empty, vm.DetailText);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Csv_BuildsDataTableWithHeader()
    {
        var path = CreateTempFile(".csv", "nombre,edad\nAna,30\nLuis,41\n");

        try
        {
            var vm = CreateSut();
            await vm.Load(path);

            Assert.True(vm.IsFileLoaded);
            Assert.True(vm.IsCsv);
            Assert.False(vm.IsText);
            Assert.False(vm.IsPdfosse);
            Assert.NotNull(vm.CsvTable);
            Assert.Equal(new[] { "nombre", "edad" }, vm.CsvTable!.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            Assert.Equal(2, vm.CsvTable.Rows.Count);
            Assert.Equal("Ana", vm.CsvTable.Rows[0][0]);
            Assert.Equal("30", vm.CsvTable.Rows[0][1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Csv_RaggedRow_PadsMissingCellsWithEmptyString()
    {
        var path = CreateTempFile(".csv", "nombre,edad\nAna\nLuis,41\n");

        try
        {
            var vm = CreateSut();
            await vm.Load(path);

            Assert.True(vm.IsCsv);
            Assert.Equal(2, vm.CsvTable!.Rows.Count);
            Assert.Equal(string.Empty, vm.CsvTable.Rows[0][1]);
            Assert.Equal("Luis", vm.CsvTable.Rows[1][0]);
            Assert.Equal("41", vm.CsvTable.Rows[1][1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Csv_EmptyContentShowsEmptyHintAndNoTable()
    {
        var path = CreateTempFile(".csv", string.Emptyhedera);

        try
        {
            _languageServiceMock
                .Setup(s => s.GetString("DocumentReaderEmptyText"))
                .Returns("El documento está vacío.");

            var vm = CreateSut();
            await vm.Load(path);

            Assert.True(vm.IsFileLoaded);
            Assert.False(vm.IsCsv);
            Assert.Null(vm.CsvTable);
            Assert.Equal(string.Empty, vm.ContentText);
            Assert.Equal("El documento está vacío.", vm.DetailText);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Docx_ConvertsToPdfStream()
    {
        var path = CreateTempFile(".docx", string.Empty);
        var pdfStream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        try
        {
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(path, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pdfStream);

            var vm = CreateSut();
            await vm.Load(path);

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
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_Docx_ConverterReturnsNull_SetsErrorState()
    {
        var path = CreateTempFile(".docx", string.Empty);

        try
        {
            _documentPdfConverterMock
                .Setup(s => s.ConvertToPdfAsync(path, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MemoryStream?)null);

            var vm = CreateSut();
            await vm.Load(path);

            Assert.False(vm.IsFileLoaded);
            Assert.False(vm.IsPdf);
            Assert.Equal(string.Empty, vm.FileName);
            Assert.Null(vm.PdfDocumentStream);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Unload_ReleasesStreamAndResetsState()
    {
        var vm = CreateSut();
        vm.Unload();

        Assert.False(vm.IsFileLoaded);
        Assert.False(vm.IsText);
        Assert.False(vm.IsCsv);
        Assert.False(vm.IsPdf);
        Assert.Equal(string.Empty, vm.FileName);
        Assert.Equal(string.Empty, vm.ContentText);
        Assert.Null(vm.CsvTable);
        Assert.Null(vm.PdfDocumentStream);
    }
}
