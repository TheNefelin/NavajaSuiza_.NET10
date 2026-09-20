using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Core.ViewModels;

public partial class DocumentReaderViewModel : BaseViewModel
{
    private const string CsvExtension = ".csv";

    private readonly ILogger<DocumentReaderViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly IFilePickerService _filePickerService;

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DetailText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ContentText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HintText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsFileLoaded { get; set; }

    public DocumentReaderViewModel(
        ILogger<DocumentReaderViewModel> logger,
        ILanguageService languageService,
        IFilePickerService filePickerService)
    {
        _logger = logger;
        _languageService = languageService;
        _filePickerService = filePickerService;
        HintText = _languageService.GetString("DocumentReaderEmptyHintText");
    }

    [RelayCommand]
    private async Task OpenDocument()
    {
        var path = await _filePickerService.PickDocumentAsync(_languageService.GetString("DocumentReaderPickerTitleText"));
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            var text = await TextFileDecoder.ReadTextAsync(path).ConfigureAwait(true);

            FileName = Path.GetFileName(path);

            if (string.Equals(Path.GetExtension(path), CsvExtension, StringComparison.OrdinalIgnoreCase))
            {
                var rows = CsvParser.Parse(text);

                if (rows.Count == 0)
                {
                    DetailText = _languageService.GetString("DocumentReaderEmptyText");
                    ContentText = string.Empty;
                }
                else
                {
                    var maxColumns = rows.Max(row => row.Length);
                    DetailText = string.Format(
                        _languageService.GetString("DocumentReaderCsvRowsColumnsText"),
                        rows.Count,
                        maxColumns);
                    ContentText = string.Join(Environment.NewLine, rows.Select(row => string.Join(" | ", row)));
                }
            }
            else
            {
                DetailText = string.Empty;
                ContentText = text;
            }

            HintText = string.Empty;
            IsFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir el documento {Path}", path);
            FileName = string.Empty;
            DetailText = string.Empty;
            ContentText = string.Empty;
            HintText = _languageService.GetString("DocumentReaderOpenErrorText");
            IsFileLoaded = false;
        }
    }
}