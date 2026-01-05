using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class TunerViewModel : BaseViewModel
{
    private readonly ILogger<TunerViewModel> _logger;
    private readonly Dictionary<string, Border> _stringBorders = new();
    private Border? _currentlyVibrating;
    private bool _isVibrating;
    private int _vibrationCountdown;
    private const int VIBRATION_COUNT = 8;
    private const double VIBRATION_DURATION = 0.05;

    public TunerViewModel(ILogger<TunerViewModel> logger)
    {
        _logger = logger;
    }

    public void RegisterStringBorder(string note, Border border)
    {
        _stringBorders[note] = border;
    }

    [RelayCommand]
    private void StringTapped(string? note)
    {
        if (string.IsNullOrEmpty(note)) return;
        if (!_stringBorders.TryGetValue(note, out var border))
        {
            _logger.LogWarning($"StringTapped: No border found for note {note}");
            return;
        }

        VibrateString(border);
    }

    [RelayCommand]
    private void StopAll()
    {
        if (_currentlyVibrating != null)
        {
            StopVibration(_currentlyVibrating);
        }
    }

    private void VibrateString(Border stringBorder)
    {
        try
        {
            // Detener vibración anterior si existe
            if (_isVibrating && _currentlyVibrating != null)
            {
                StopVibration(_currentlyVibrating);
            }

            _currentlyVibrating = stringBorder;
            _isVibrating = true;
            _vibrationCountdown = VIBRATION_COUNT;

            PerformVibration();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in VibrateString: {ex.Message}");
        }
    }

    private void PerformVibration()
    {
        if (!_isVibrating || _currentlyVibrating == null)
        {
            return;
        }

        var border = _currentlyVibrating;

        var vibrationAnimation = new Animation();

        // Definir los pasos de la vibración directamente en el Border
        vibrationAnimation.Add(0.0, 0.25, new Animation(v => border.TranslationY = v, 0, -5, Easing.Linear));
        vibrationAnimation.Add(0.25, 0.50, new Animation(v => border.TranslationY = v, -5, 5, Easing.Linear));
        vibrationAnimation.Add(0.50, 0.75, new Animation(v => border.TranslationY = v, 5, -5, Easing.Linear));
        vibrationAnimation.Add(0.75, 1.0, new Animation(v => border.TranslationY = v, -5, 0, Easing.Linear));

        vibrationAnimation.Commit(
            border,
            "Vibration",
            length: (uint)(VIBRATION_DURATION * 1000),
            finished: (v, cancelled) =>
            {
                _vibrationCountdown--;

                if (_vibrationCountdown > 0 && _isVibrating)
                {
                    PerformVibration();
                }
                else
                {
                    if (_currentlyVibrating != null)
                    {
                        StopVibration(_currentlyVibrating);
                    }
                }
            });
    }

    private void StopVibration(Border stringBorder)
    {
        try
        {
            stringBorder.AbortAnimation("Vibration");
            stringBorder.TranslationY = 0;

            _isVibrating = false;
            _vibrationCountdown = 0;
            _currentlyVibrating = null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in StopVibration: {ex.Message}");
        }
    }
}