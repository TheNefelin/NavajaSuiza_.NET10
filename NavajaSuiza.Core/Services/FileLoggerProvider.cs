using Microsoft.Extensions.Logging;
using System.Text;

namespace NavajaSuiza.Core.Services;

/// <summary>
/// Proveedor de logging que agrega cada entrada como una línea a un archivo de texto.
/// Pensado para diagnóstico en dispositivos donde no hay consola ni depurador disponibles.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private const long DefaultMaxFileSizeBytes = 512 * 1024;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly object _syncRoot = new();
    private readonly long _maxFileSizeBytes;

    public FileLoggerProvider(string filePath, long maxFileSizeBytes = DefaultMaxFileSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("La ruta del archivo de log es obligatoria.", nameof(filePath));
        if (maxFileSizeBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFileSizeBytes));

        FilePath = filePath;
        _maxFileSizeBytes = maxFileSizeBytes;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public string FilePath { get; }

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, categoryName);

    void IDisposable.Dispose()
    {
    }

    private void Write(string categoryName, LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var builder = new StringBuilder();
        builder.Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
        builder.Append(" [").Append(logLevel).Append(']');
        builder.Append(" [").Append(categoryName).Append(']');
        if (eventId.Id != 0)
            builder.Append(" (").Append(eventId.Id).Append(')');
        builder.Append(' ').Append(message);
        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        builder.AppendLine();

        var line = builder.ToString();

        try
        {
            lock (_syncRoot)
            {
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length >= _maxFileSizeBytes)
                    File.Delete(FilePath);

                File.AppendAllText(FilePath, line, Utf8NoBom);
            }
        }
        catch
        {
            // El logging de diagnóstico nunca debe interrumpir la aplicación.
        }
    }

    private sealed class FileLogger : ILogger
    {
        private readonly FileLoggerProvider _provider;
        private readonly string _categoryName;

        public FileLogger(FileLoggerProvider provider, string categoryName)
        {
            _provider = provider;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter is null)
                return;

            _provider.Write(_categoryName, logLevel, eventId, formatter(state, exception), exception);
        }
    }
}
