using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class FileLoggerProviderTests : IDisposable
{
    private readonly string _tempDirectory;

    public FileLoggerProviderTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "NavajaSuizaFileLogger_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, recursive: true);
        }
        catch
        {
            // La limpieza del directorio temporal no debe hacer fallar la prueba.
        }
    }

    private string NewFilePath() => Path.Combine(_tempDirectory, "NavajaSuiza.log");

    private static void Log(ILogger logger, LogLevel level, string message, Exception? exception = null)
    {
        logger.Log(level, new EventId(1, "Test"), message, exception, static (state, _) => state);
    }

    [Fact]
    public void Constructor_WithEmptyPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => new FileLoggerProvider(" "));
    }

    [Fact]
    public void Constructor_CreatesMissingDirectory()
    {
        var path = NewFilePath();
        Assert.False(Directory.Exists(_tempDirectory));

        _ = new FileLoggerProvider(path);

        Assert.True(Directory.Exists(_tempDirectory));
    }

    [Fact]
    public void Log_WritesCategoryLevelAndMessage()
    {
        var path = NewFilePath();
        var provider = new FileLoggerProvider(path);
        var logger = provider.CreateLogger("NavajaSuiza.Test.Categoria");

        Log(logger, LogLevel.Warning, "mensaje de prueba");

        var content = File.ReadAllText(path);
        Assert.Contains("NavajaSuiza.Test.Categoria", content);
        Assert.Contains("Warning", content);
        Assert.Contains("mensaje de prueba", content);
    }

    [Fact]
    public void Log_WritesExceptionDetails()
    {
        var path = NewFilePath();
        var logger = new FileLoggerProvider(path).CreateLogger("Cat");

        Log(logger, LogLevel.Critical, "fallo", new InvalidOperationException("motivo del fallo"));

        var content = File.ReadAllText(path);
        Assert.Contains("fallo", content);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("motivo del fallo", content);
    }

    [Fact]
    public void Log_AppendsSuccessiveEntries()
    {
        var path = NewFilePath();
        var logger = new FileLoggerProvider(path).CreateLogger("Cat");

        Log(logger, LogLevel.Information, "primera");
        Log(logger, LogLevel.Information, "segunda");

        var content = File.ReadAllText(path);
        Assert.Contains("primera", content);
        Assert.Contains("segunda", content);
    }

    [Fact]
    public void Log_WhenFileExceedsLimit_ResetsFile()
    {
        var path = NewFilePath();
        var provider = new FileLoggerProvider(path, maxFileSizeBytes: 256);
        var logger = provider.CreateLogger("Cat");

        Log(logger, LogLevel.Information, new string('x', 400));
        Assert.True(new FileInfo(path).Length > 256);

        Log(logger, LogLevel.Information, "contenido tras el recorte");

        var content = File.ReadAllText(path);
        Assert.DoesNotContain(new string('x', 400), content);
        Assert.Contains("contenido tras el recorte", content);
    }

    [Theory]
    [InlineData(LogLevel.Trace, true)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Critical, true)]
    [InlineData(LogLevel.None, false)]
    public void IsEnabled_ReturnsExpected(LogLevel level, bool expected)
    {
        var logger = new FileLoggerProvider(NewFilePath()).CreateLogger("Cat");
        Assert.Equal(expected, logger.IsEnabled(level));
    }

    [Fact]
    public void BeginScope_ReturnsNull()
    {
        var logger = new FileLoggerProvider(NewFilePath()).CreateLogger("Cat");
        Assert.Null(logger.BeginScope("scope"));
    }
}
