using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class TextReaderViewModel : BaseViewModel
{
    private const int MaxBytesToRead = 2 * 1024 * 1024;

    private readonly ILogger<TextReaderViewModel> _logger;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    public partial string Content { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFileLoaded { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public TextReaderViewModel(ILogger<TextReaderViewModel> logger, ILanguageService languageService)
    {
        _logger = logger;
        _languageService = languageService;
    }

    public void Load(string path)
    {
        try
        {
            _logger.LogInformation("Abriendo archivo de texto: {Path}", path);

            var content = ReadAsText(path);

            Content = content;
            FileName = Path.GetFileName(path);
            IsFileLoaded = true;
            Message = string.Empty;
        }
        catch
        {
            _logger.LogError($"Error al abrir el archivo de texto {path}");
            Content = string.Empty;
            FileName = string.Empty;
            IsFileLoaded = false;
            Message = _languageService.GetString("TextReaderOpenErrorText");
        }
    }

    public void Unload()
    {
        Content = string.Empty;
        FileName = string.Empty;
        IsFileLoaded = false;
        Message = string.Empty;
    }

    private string ReadAsText(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length > MaxBytesToRead)
        {
            throw new InvalidOperationException();
        }

        using var reader = new StreamReader(
            stream,
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        return reader.ReadToEnd();
    }
}