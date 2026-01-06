using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class TunerViewModel : BaseViewModel
{
    private readonly ILogger<TunerViewModel> _logger;
    private readonly Dictionary<string, Border> _stringBorders = new();
    private readonly Dictionary<string, string> _stringAudioFiles = new()
        {
            { "GN_E2", "GN_01_E2.wav" },
            { "GN_A2", "GN_02_A2.wav" },
            { "GN_D3", "GN_03_D3.wav" },
            { "GN_G3", "GN_04_G3.wav" },
            { "GN_B3", "GN_05_B3.wav" },
            { "GN_E4", "GN_06_E4.wav" },
            { "GS_E2", "GN_01_E2.wav" },
            { "GS_A2", "GN_02_A2.wav" },
            { "GS_D3", "GN_03_D3.wav" },
            { "GS_G3", "GN_04_G3.wav" },
            { "GS_B3", "GN_05_B3.wav" },
            { "GS_E4", "GN_06_E4.wav" }
        };
    private MediaElement _mediaElement;
    private Border? _currentlyVibrating;
    private bool _isVibrating;
    private int _vibrationCountdown;
    private const int VIBRATION_COUNT = 8;
    private const double VIBRATION_DURATION = 0.05;

    public TunerViewModel(ILogger<TunerViewModel> logger)
    {
        _logger = logger;
    }

    public void RegisterTunerMediaElement(MediaElement mediaElement)
    {
        _mediaElement = mediaElement;
    }

    public void RegisterStringBorder(string note, Border border)
    {
        _stringBorders[note] = border;
    }

    [RelayCommand]
    private async void StringTapped(string? note)
    {
        if (string.IsNullOrEmpty(note)) return;
        if (!_stringBorders.TryGetValue(note, out var border))
        {
            _logger.LogWarning($"StringTapped: No border found for note {note}");
            return;
        }

        VibrateString(border);
        await PlayStringSound(note);
    }

    private async Task PlayStringSound(string note)
    {
        try
        {
            if (!_stringAudioFiles.TryGetValue(note, out var audioFile))
            {
                _logger.LogWarning($"PlayStringSound: No audio file found for note {note}");
                return;
            }

            _logger.LogInformation($"Playing sound for note {note} from file {audioFile}");
            _mediaElement.Stop();
            _mediaElement.Source = MediaSource.FromResource(audioFile);
            _mediaElement.Play();
        }
        catch (Exception ex)
        {
            _logger.LogError($"[TunerViewModel] Error in PlayStringSound: {ex.Message}");
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        _mediaElement.Stop();
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
            "Vibration",  // Nombre único para poder detenerlo
            length: (uint)(VIBRATION_DURATION * 1000),
            repeat: () => _isVibrating,  // ¡SOLO ESTA LÍNEA HACE EL LOOP!
            finished: (v, cancelled) =>
            {
                // Solo se ejecuta si se cancela explícitamente
                if (!cancelled && _isVibrating)
                {
                    // Esto no debería pasar, pero por seguridad:
                    _logger.LogWarning("Vibration finished unexpectedly");
                }
            }
        );
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