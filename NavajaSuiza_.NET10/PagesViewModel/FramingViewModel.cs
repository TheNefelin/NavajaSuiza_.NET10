using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class FramingViewModel : BaseViewModel
{
    // 1:1 (cuadrado) BtnSquare
    // 4:5 (vertical) Portrait
    // 9:16 (vertical stories) Stories
    // 16:9 (horizontal) Landscape

    [ObservableProperty]
    private string _aspectRatio = "1:1";

    [ObservableProperty]
    private string _aspectRatioName = "Square";

    [ObservableProperty]
    private string _fitMode = "Contain";

    [ObservableProperty]
    private string _canvasBackground = "Blur";

    [ObservableProperty]
    private int _blurIntensity = 50;

    [RelayCommand]
    private async void SetAspectRatio(string ratio)
    {
        AspectRatio = ratio;
        AspectRatioName = "Otra Dimension";
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
}
