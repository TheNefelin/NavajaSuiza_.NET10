using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class PizarraViewModel : BaseViewModel
{
    private const double PenContrastThreshold = 2.5d;
    private const string DarkBoardPenHex = "#FDD835";
    private const string LightBoardPenHex = "#1F1F1F";

    private readonly ILogger<PizarraViewModel> _logger;

    public ObservableCollection<PizarraStroke> Strokes { get; } = new();

    [ObservableProperty]
    public partial string SelectedColor { get; set; } = "#E53935";

    [ObservableProperty]
    public partial double StrokeWidth { get; set; } = 4d;

    [ObservableProperty]
    public partial string BoardColor { get; set; } = "#F2F2EE";

    [ObservableProperty]
    public partial bool IsColorPickerVisible { get; set; }

    public event Action? CanvasChanged;

    public PizarraViewModel(ILogger<PizarraViewModel> logger)
    {
        _logger = logger;
    }

    public void StartStroke(float x, float y)
    {
        var stroke = new PizarraStroke
        {
            ColorHex = SelectedColor,
            Width = (float)StrokeWidth
        };
        stroke.AddPoint(x, y);
        Strokes.Add(stroke);
        RaiseCanvasChanged();
    }

    public void AddPoint(float x, float y)
    {
        if (Strokes.Count == 0)
            return;

        Strokes[^1].AddPoint(x, y);
        RaiseCanvasChanged();
    }

    /// <summary>
    /// Marca el fin del trazo actual. No requiere acción adicional en esta fase.
    /// </summary>
    public void EndStroke()
    {
    }

    [RelayCommand]
    private void SetColor(string hex)
    {
        SelectedColor = hex;
    }

    [RelayCommand]
    private void OpenColorPicker()
    {
        IsColorPickerVisible = true;
    }

    [RelayCommand]
    private void CloseColorPicker()
    {
        IsColorPickerVisible = false;
    }

    [RelayCommand]
    private void SelectColor(string hex)
    {
        SelectedColor = hex;
        IsColorPickerVisible = false;
    }

    [RelayCommand]
    private void SetBoardColor(string hex)
    {
        BoardColor = hex;
        EnsurePenContrast();
        RaiseCanvasChanged();
    }

    private void EnsurePenContrast()
    {
        if (ContrastRatio(BoardColor, SelectedColor) >= PenContrastThreshold)
            return;

        var isDarkBoard = RelativeLuminance(BoardColor) < 0.5d;
        SelectedColor = isDarkBoard ? DarkBoardPenHex : LightBoardPenHex;
    }

    private static double ContrastRatio(string hex1, string hex2)
    {
        var lighter = Math.Max(RelativeLuminance(hex1), RelativeLuminance(hex2));
        var darker = Math.Min(RelativeLuminance(hex1), RelativeLuminance(hex2));
        return (lighter + 0.05d) / (darker + 0.05d);
    }

    private static double RelativeLuminance(string hex)
    {
        var r = Linearize(Channel(hex, 1));
        var g = Linearize(Channel(hex, 3));
        var b = Linearize(Channel(hex, 5));
        return 0.2126d * r + 0.7152d * g + 0.0722d * b;
    }

    private static double Channel(string hex, int startIndex) =>
        Convert.ToInt32(hex.Substring(startIndex, 2), 16) / 255d;

    private static double Linearize(double channel) =>
        channel <= 0.03928d
            ? channel / 12.92d
            : Math.Pow((channel + 0.055d) / 1.055d, 2.4d);

    [RelayCommand]
    private void Clear()
    {
        Strokes.Clear();
        RaiseCanvasChanged();
        _logger.LogInformation("Pizarra limpiada");
    }

    [RelayCommand]
    private void Undo()
    {
        if (Strokes.Count == 0)
            return;

        Strokes.RemoveAt(Strokes.Count - 1);
        RaiseCanvasChanged();
        _logger.LogInformation("Pizarra: trazo deshecho");
    }

    private void RaiseCanvasChanged() => CanvasChanged?.Invoke();
}