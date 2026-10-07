using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class BoardViewModel : BaseViewModel
{
    private const double PenContrastThreshold = 2.5d;
    private const string DarkBoardPenHex = "#FDD835";
    private const string LightBoardPenHex = "#1F1F1F";

    private readonly ILogger<BoardViewModel> _logger;
    private readonly IBoardImageExporter _exporter;
    private readonly List<BoardUndoEntry> _undoHistory = new();

    public ObservableCollection<BoardStroke> Strokes { get; } = new();

    public ObservableCollection<BoardText> Texts { get; } = new();

    [ObservableProperty]
    public partial bool IsTextModeActive { get; set; }

    [ObservableProperty]
    public partial string SelectedColor { get; set; } = "#E53935";

    [ObservableProperty]
    public partial double StrokeWidth { get; set; } = 4d;

    [ObservableProperty]
    public partial string BoardColor { get; set; } = "#F2F2EE";

    [ObservableProperty]
    public partial bool IsColorPickerVisible { get; set; }

    public BoardExportResult LastExportResult { get; private set; } = BoardExportResult.Saved;

    public event Action? CanvasChanged;

    public BoardViewModel(ILogger<BoardViewModel> logger, IBoardImageExporter exporter)
    {
        _logger = logger;
        _exporter = exporter;
    }

    public void StartStroke(float x, float y)
    {
        if (IsTextModeActive)
            return;

        var stroke = new BoardStroke
        {
            ColorHex = SelectedColor,
            Width = (float)StrokeWidth
        };
        stroke.AddPoint(x, y);
        Strokes.Add(stroke);
        _undoHistory.Add(BoardUndoEntry.ForStroke(stroke));
        RaiseCanvasChanged();
    }

    public void AddPoint(float x, float y)
    {
        if (IsTextModeActive || Strokes.Count == 0)
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

    /// <summary>
    /// Agrega un texto al lienzo en modo texto. No hace nada si el modo está
    /// desactivado o si el contenido está vacío.
    /// </summary>
    public bool AddText(string content, float x, float y)
    {
        if (!IsTextModeActive || string.IsNullOrWhiteSpace(content))
            return false;

        var text = new BoardText
        {
            Content = content.Trim(),
            X = x,
            Y = y,
            FontSize = BoardDefaults.FontSize,
            ColorHex = SelectedColor
        };

        Texts.Add(text);
        _undoHistory.Add(BoardUndoEntry.ForText(text));
        RaiseCanvasChanged();
        _logger.LogInformation("Board: text added");
        return true;
    }

    [RelayCommand]
    private void ToggleTextMode()
    {
        IsTextModeActive = !IsTextModeActive;
        _logger.LogInformation("Board: text mode {State}", IsTextModeActive ? "on" : "off");
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
        if (ContrastRatio(BoardColor, SelectedColor) < PenContrastThreshold)
        {
            var isDarkBoard = RelativeLuminance(BoardColor) < 0.5d;
            SelectedColor = isDarkBoard ? DarkBoardPenHex : LightBoardPenHex;
        }

        ApplyContrastToTexts();
    }

    /// <summary>
    /// Ajusta el color de los textos ya creados cuando el tablero cambia, para que
    /// no queden invisibles sobre el fondo nuevo.
    /// </summary>
    private void ApplyContrastToTexts()
    {
        var isDarkBoard = RelativeLuminance(BoardColor) < 0.5d;
        var readable = isDarkBoard ? DarkBoardPenHex : LightBoardPenHex;

        foreach (var text in Texts)
        {
            if (ContrastRatio(BoardColor, text.ColorHex) < PenContrastThreshold)
                text.ColorHex = readable;
        }
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
        Texts.Clear();
        _undoHistory.Clear();
        RaiseCanvasChanged();
        _logger.LogInformation("Board cleared");
    }

    /// <summary>
    /// Deshace la última acción, sea un trazo o un texto. El historial mantiene
    /// el orden cronológico, así que deshacer sigue el orden en que el usuario
    /// creó los elementos aunque estén en colecciones separadas.
    /// </summary>
    [RelayCommand]
    private void Undo()
    {
        if (_undoHistory.Count == 0)
            return;

        var entry = _undoHistory[^1];
        _undoHistory.RemoveAt(_undoHistory.Count - 1);

        if (entry.Stroke is not null)
        {
            Strokes.Remove(entry.Stroke);
            _logger.LogInformation("Board: stroke undone");
        }
        else if (entry.Text is not null)
        {
            Texts.Remove(entry.Text);
            _logger.LogInformation("Board: text undone");
        }

        RaiseCanvasChanged();
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Strokes.Count == 0 && Texts.Count == 0)
        {
            LastExportResult = BoardExportResult.Failed;
            return;
        }

        LastExportResult = await _exporter.ExportAsync(Strokes.ToArray(), Texts.ToArray(), BoardColor);
        _logger.LogInformation("Board export: {Result}", LastExportResult);
    }

    private void RaiseCanvasChanged() => CanvasChanged?.Invoke();

    private readonly record struct BoardUndoEntry(BoardStroke? Stroke, BoardText? Text)
    {
        public static BoardUndoEntry ForStroke(BoardStroke stroke) => new(stroke, null);

        public static BoardUndoEntry ForText(BoardText text) => new(null, text);
    }
}