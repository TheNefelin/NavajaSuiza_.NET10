using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class PdfReaderViewModel : BaseViewModel
{
    private readonly ILogger<PdfReaderViewModel> _logger;

    [ObservableProperty]
    public partial Stream? PdfDocumentStream { get; set; }

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFileLoaded { get; set; }

    public PdfReaderViewModel(ILogger<PdfReaderViewModel> logger)
    {
        _logger = logger;
    }

    public void Load(string path)
    {
        try
        {
            _logger.LogInformation("Abriendo PDF: {Path}", path);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            Unload();
            PdfDocumentStream = stream;
            FileName = Path.GetFileName(path);
            IsFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir el PDF {Path}", path);
            Unload();
        }
    }

    public void Load(Stream stream, string fileName)
    {
        try
        {
            _logger.LogInformation("Abriendo PDF desde stream: {FileName} ({Bytes} bytes)", fileName, stream.Length);

            Unload();
            PdfDocumentStream = stream;
            FileName = fileName;
            IsFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir el PDF {FileName}", fileName);
            Unload();
        }
    }

    public void Unload()
    {
        PdfDocumentStream?.Dispose();
        PdfDocumentStream = null;
        FileName = string.Empty;
        IsFileLoaded = false;
    }
}