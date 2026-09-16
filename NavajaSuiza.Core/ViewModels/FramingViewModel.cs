using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class FramingViewModel : BaseViewModel
{
    private readonly ILogger<FramingViewModel> _logger;
    private readonly ILanguageService _languageService;
    private readonly IImagePickerService _imagePickerService;

    [ObservableProperty]
    public partial string AspectRatioName { get; set; } = "";

    [ObservableProperty]
    public partial string AspectRatio { get; set; } = AppConstants.Framing.DEFAULT_ASPECT_RATIO;

    [ObservableProperty]
    public partial string AspectMode { get; set; } = AppConstants.Framing.DEFAULT_ASPECT_MODE;

    [ObservableProperty]
    public partial string CanvasBackground { get; set; } = AppConstants.Framing.DEFAULT_CANVAS_BACKGROUND;

    [ObservableProperty]
    public partial string CanvasBackgroundColor { get; set; } = AppConstants.Framing.DEFAULT_CANVAS_BACKGROUND_COLOR;

    [ObservableProperty]
    public partial double CanvasBackgroundOpacity { get; set; } = AppConstants.Framing.DEFAULT_CANVAS_BACKGROUND_OPACITY;

    [ObservableProperty]
    public partial double CanvasWidth { get; set; }

    [ObservableProperty]
    public partial double CanvasHeight { get; set; }

    [ObservableProperty]
    public partial int BlurIntensity { get; set; } = AppConstants.Framing.DEFAULT_BLUR_INTENSITY;

    [ObservableProperty]
    public partial string? LoadedImage { get; set; }

    private readonly Dictionary<string, double> _aspectRatios = AppConstants.Framing.AspectRatios;

    public FramingViewModel(
        ILogger<FramingViewModel> logger,
        ILanguageService languageService,
        IImagePickerService imagePickerService)
    {
        _logger = logger;
        _languageService = languageService;
        _imagePickerService = imagePickerService;
        _languageService.LanguageChanged += OnLanguageChanged;

        UpdateAspectRatioName(AspectRatio);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateAspectRatioName(AspectRatio);
    }

    public override void Cleanup()
    {
        _languageService.LanguageChanged -= OnLanguageChanged;
        base.Cleanup();
    }

    private void UpdateAspectRatioName(string ratio)
    {
        AspectRatioName = ratio switch
        {
            AppConstants.Framing.DEFAULT_ASPECT_RATIO => _languageService.GetString("FramingAspectRatioSquareText"),
            "4:5" => _languageService.GetString("FramingAspectRatioPortraitText"),
            "9:16" => _languageService.GetString("FramingAspectRatioStoriesText"),
            "16:9" => _languageService.GetString("FramingAspectRatioLandscapeText"),
            _ => "Unknown"
        };
    }

    public void SetCanvasWidth(double width)
    {
        CanvasWidth = width;
        CanvasHeight = width;
    }

    [RelayCommand]
    private void SetAspectRatio(string ratio)
    {
        AspectRatio = ratio;
        UpdateAspectRatioName(ratio);

        if (_aspectRatios.TryGetValue(ratio, out var heightMultiplier))
        {
            CanvasHeight = CanvasWidth * heightMultiplier;
        }
    }

    [RelayCommand]
    private void SetAspectMode(string mode)
    {
        AspectMode = mode;
    }

    [RelayCommand]
    private void SetCanvasBackground(string background)
    {
        CanvasBackground = background;

        if (background == AppConstants.Framing.DEFAULT_CANVAS_BACKGROUND)
        {
            CanvasBackgroundColor = "Transparent";
            CanvasBackgroundOpacity = AppConstants.Framing.DEFAULT_CANVAS_BACKGROUND_OPACITY;
        }
        else
        {
            CanvasBackgroundColor = CanvasBackground;
            CanvasBackgroundOpacity = 0.0;
        }
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        var path = await _imagePickerService.PickImageAsync("Selecciona una imagen");

        if (path == null) return;

        LoadedImage = path;
    }
}
