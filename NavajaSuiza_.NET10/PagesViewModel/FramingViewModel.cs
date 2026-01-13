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
    private string _aspectRatioName = "";

    [ObservableProperty]
    private string _aspectRatio = "1:1";

    [ObservableProperty]
    private string _aspectMode = "AspectFill";

    [ObservableProperty]
    private string _canvasBackground = "Blur";

    [ObservableProperty]
    private string _canvasBackgroundColor = "Black";

    [ObservableProperty]
    private double _canvasBackgroundOpacity = 1.0f;

    [ObservableProperty]
    private double _canvasWidth;

    [ObservableProperty]
    private double _canvasHeight;

    private const double MAX_CANVAS_WIDTH = 500;

    [ObservableProperty]
    private int _blurIntensity = 13; // max 25

    [ObservableProperty]
    private ImageSource _loadedImage;

    private Dictionary<string, double> _aspectRatios = new()
    {
        { "1:1", 1.0 },      // alto/ancho
        { "4:5", 1.25 },
        { "9:16", 1.777 },
        { "16:9", 0.5625 }
    };

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

    public void SetCanvasWidth(double width)
    {
        CanvasWidth = width;
        CanvasHeight = width;
    }      

    [RelayCommand]
    private async void SetAspectRatio(string ratio)
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
    private async void SetAspectMode(string mode)
    {
        AspectMode = mode;
    }

    [RelayCommand]
    private async void SetCanvasBackground(string background)
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