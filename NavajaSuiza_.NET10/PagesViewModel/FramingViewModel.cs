using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class FramingViewModel : BaseViewModel
{
    // 1:1 (cuadrado) BtnSquare
    // 4:5 (vertical) Portrait
    // 9:16 (vertical stories) Stories
    // 16:9 (horizontal) Landscape

    private readonly ILogger<FramingViewModel> _logger;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    private string _aspectRatio = "1:1";

    [ObservableProperty]
    private string _aspectRatioName = "";

    [ObservableProperty]
    private string _fitMode = "Contain";

    [ObservableProperty]
    private string _canvasBackground = "Blur";

    [ObservableProperty]
    private int _blurIntensity = 50;

    [ObservableProperty]
    private ImageSource _loadedImage;

    public FramingViewModel(
        ILogger<FramingViewModel> logger,
        IImageProcessingService imageProcessingService,
        ILanguageService languageService)
    {
        _logger = logger;
        _imageProcessingService = imageProcessingService;
        _languageService = languageService;

        _languageService.LanguageChanged += OnLanguageChanged;
        UpdateAspectRatioName(AspectRatio);
    }

    [RelayCommand]
    private async void SetAspectRatio(string ratio)
    {
        AspectRatio = ratio;
        UpdateAspectRatioName(ratio);
    }

    [RelayCommand]
    private async void SetFitMode(string mode)
    {
        FitMode = mode;
    }

    [RelayCommand]
    private async void SetCanvasBackground(string background)
    {
        CanvasBackground = background;
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        try
        {
            _logger.LogInformation("[FramingViewModel] - Loading image...");
            LoadedImage = await _imageProcessingService.LoadImageAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FramingViewModel] - Error loading image");
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        UpdateAspectRatioName(AspectRatio);
    }

    private void UpdateAspectRatioName(string ratio)
    {
        AspectRatioName = ratio switch
        {
            "1:1" => _languageService.GetString("FramingAspectRatioSquareText"),
            "4:5" => _languageService.GetString("FramingAspectRatioPortraitText"),
            "9:16" => _languageService.GetString("FramingAspectRatioStoriesText"),
            "16:9" => _languageService.GetString("FramingAspectRatioLandscapeText"),
            _ => "Unknown"
        };
    }
}
