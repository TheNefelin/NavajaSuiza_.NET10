using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class FramingViewModel : BaseViewModel
{
    // 1:1 (cuadrado) BtnSquare
    // 4:5 (vertical) Portrait
    // 9:16 (vertical stories) Stories
    // 16:9 (horizontal) Landscape

    private readonly ILogger<FramingViewModel> _logger;
    private readonly ILanguageService _languageService;

    [ObservableProperty]
    public partial string AspectRatioName { get; set; } = "";

    [ObservableProperty]
    public partial string AspectRatio { get; set; } = "1:1";

    [ObservableProperty]
    public partial string AspectMode { get; set; } = "AspectFill";

    [ObservableProperty]
    public partial string CanvasBackground { get; set; } = "Blur";

    [ObservableProperty]
    public partial string CanvasBackgroundColor { get; set; } = "Black";

    [ObservableProperty]
    public partial double CanvasBackgroundOpacity { get; set; } = 1.0f;

    [ObservableProperty]
    public partial double CanvasWidth { get; set; }

    [ObservableProperty]
    public partial double CanvasHeight { get; set; }

    private const double MAX_CANVAS_WIDTH = 500;

    [ObservableProperty]
    public partial int BlurIntensity { get; set; } = 13;

    [ObservableProperty]
    public partial ImageSource? LoadedImage { get; set; }

    private Dictionary<string, double> _aspectRatios = new()
    {
        { "1:1", 1.0 },      // alto/ancho
        { "4:5", 1.25 },
        { "9:16", 1.777 },
        { "16:9", 0.5625 }
    };

    public FramingViewModel(
        ILogger<FramingViewModel> logger,
        ILanguageService languageService)
    {
        _logger = logger;
        _languageService = languageService;
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
            "1:1" => _languageService.GetString("FramingAspectRatioSquareText"),
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

        // Calcular nueva altura
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

        if (background == "Blur")
        {
            CanvasBackgroundColor = "Transparent";
            CanvasBackgroundOpacity = 1.0f;
        } 
        else
        {
            CanvasBackgroundColor = CanvasBackground;
            CanvasBackgroundOpacity = 0.0f;
        }
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (result == null) return;
        
        LoadedImage = ImageSource.FromFile(result.FullPath);
    }
}