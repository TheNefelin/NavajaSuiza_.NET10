using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class InstrumentAudioService : IInstrumentAudioService
{
    private readonly ILogger<InstrumentAudioService> _logger;
    private readonly Dictionary<Border, string> _borderAudioMap = new();
    private Border? _currentlyVibrating;
    private bool _isVibrating;
    private const double VIBRATION_DURATION = 0.05;
    private MediaElement _mediaElement;

    public InstrumentAudioService(ILogger<InstrumentAudioService> logger)
    {
        _logger = logger;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _mediaElement = mediaElement;
    }

    public void RegisterStringBorder(Border border, string audioName)
    {
        _borderAudioMap[border] = audioName;
        _logger.LogInformation($"Registered border with audio {audioName}");
    }

    public async Task StringTappedAsync(Border border)
    {
        if (_borderAudioMap.TryGetValue(border, out var audioName))
        {
            StartVibration(border);
            await PlayAudioAsync(audioName);
        }
        else
        {
            _logger.LogWarning("Border not registered");
        }
    }

    public async Task StopAllStringAsync()
    {
        StopVibration(_currentlyVibrating);
        StopAudio();
    }

    private void StartVibration(Border border)
    {
        try
        {
            // Detener vibración anterior
            if (_currentlyVibrating != null)
            {
                StopVibration(_currentlyVibrating);
            }

            _currentlyVibrating = border;
            _isVibrating = true;

            var vibrationAnimation = new Animation();

            vibrationAnimation.Add(0.0, 0.25, new Animation(v => border.TranslationY = v, 0, -5, Easing.Linear));
            vibrationAnimation.Add(0.25, 0.50, new Animation(v => border.TranslationY = v, -5, 5, Easing.Linear));
            vibrationAnimation.Add(0.50, 0.75, new Animation(v => border.TranslationY = v, 5, -5, Easing.Linear));
            vibrationAnimation.Add(0.75, 1.0, new Animation(v => border.TranslationY = v, -5, 0, Easing.Linear));

            vibrationAnimation.Commit(
                border,
                "Vibration",
                length: (uint)(VIBRATION_DURATION * 1000),
                repeat: () => _isVibrating);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in VibrateString: {ex.Message}");
        }
    }

    private void StopVibration(Border stringBorder)
    {
        if (stringBorder == null) return;

        try
        {
            stringBorder.AbortAnimation("Vibration");
            stringBorder.TranslationY = 0;
            _isVibrating = false;
            _currentlyVibrating = null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in StopVibration: {ex.Message}");
        }
    }

    private async Task PlayAudioAsync(string audioName)
    {
        try
        {
            if (_mediaElement == null)
            {
                _logger.LogWarning("MediaElement not initialized");
                return;
            }

            _logger.LogInformation($"Playing audio: {audioName}");
            _mediaElement.Stop();
            _mediaElement.Source = MediaSource.FromResource(audioName);
            _mediaElement.Play();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error playing audio: {ex.Message}");
        }
    }

    private void StopAudio()
    {
        _mediaElement.Stop();
    }
}
