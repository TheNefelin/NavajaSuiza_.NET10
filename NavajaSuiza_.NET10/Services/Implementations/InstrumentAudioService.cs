using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

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

    public ObservableCollection<InstrumentStringData> GetNylonStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "E2", AudioName = "GN_01_E2.wav", Description = "82.41 Hz", Thickness = 6 },
            new InstrumentStringData { Note = "A2", AudioName = "GN_02_A2.wav", Description = "110.00 Hz", Thickness = 5 },
            new InstrumentStringData { Note = "A4", AudioName = "GN_03_D3.wav", Description = "146.83 Hz", Thickness = 4 },
            new InstrumentStringData { Note = "G3", AudioName = "GN_04_G3.wav", Description = "196.00 Hz", Thickness = 3 },
            new InstrumentStringData { Note = "B3", AudioName = "GN_05_B3.wav", Description = "246.94 Hz", Thickness = 2 },
            new InstrumentStringData { Note = "E4", AudioName = "GN_06_E4.wav", Description = "329.63 Hz", Thickness = 1 }
        };
    }

    public ObservableCollection<InstrumentStringData> GetSteelStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "E2", AudioName = "GS_01_E2.wav", Description = "82.41 Hz", Thickness = 6 },
            new InstrumentStringData { Note = "A2", AudioName = "GS_02_A2.wav", Description = "110.00 Hz", Thickness = 5 },
            new InstrumentStringData { Note = "A4", AudioName = "GS_03_D3.wav", Description = "146.83 Hz", Thickness = 4 },
            new InstrumentStringData { Note = "G3", AudioName = "GS_04_G3.wav", Description = "196.00 Hz", Thickness = 3 },
            new InstrumentStringData { Note = "B3", AudioName = "GS_05_B3.wav", Description = "246.94 Hz", Thickness = 2 },
            new InstrumentStringData { Note = "E4", AudioName = "GS_06_E4.wav", Description = "329.63 Hz", Thickness = 1 }
        };
    }

    public ObservableCollection<InstrumentStringData> GetBassStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "E1", AudioName = "B_01_E1.wav", Description = "41.20 Hz", Thickness = 6 },
            new InstrumentStringData { Note = "A1", AudioName = "B_02_A1.wav", Description = "55.00 Hz Hz", Thickness = 5 },
            new InstrumentStringData { Note = "D2", AudioName = "B_03_D2.wav", Description = "73.41 Hz", Thickness = 4 },
            new InstrumentStringData { Note = "G2", AudioName = "B_04_G2.wav", Description = "97.99 Hz", Thickness = 3 }
        };
    }

    public ObservableCollection<InstrumentStringData> GetUkeleleStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "G4", AudioName = "U_01_G4.wav", Description = "392.00 Hz", Thickness = 1 },
            new InstrumentStringData { Note = "C4", AudioName = "U_02_C4.wav", Description = "261.63 Hz", Thickness = 3 },
            new InstrumentStringData { Note = "E4", AudioName = "U_03_E4.wav", Description = "329.63 Hz", Thickness = 2 },
            new InstrumentStringData { Note = "A4", AudioName = "U_04_A4.wav", Description = "440.63 Hz", Thickness = 1 }
        };
    }

    public ObservableCollection<InstrumentStringData> GetViolinStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "G3", AudioName = "V_01_G3.wav", Description = "196.00 Hz", Thickness = 1 },
            new InstrumentStringData { Note = "D4", AudioName = "V_02_D4.wav", Description = "293.66 Hz", Thickness = 1 },
            new InstrumentStringData { Note = "A4", AudioName = "V_03_A4.wav", Description = "440.00 Hz", Thickness = 1 },
            new InstrumentStringData { Note = "E5", AudioName = "V_04_E5.wav", Description = "659.26 Hz", Thickness = 1 }
        };
    }

    public ObservableCollection<InstrumentStringData> GetCharangoStringConfig()
    {
        return new ObservableCollection<InstrumentStringData>()
        {
            new InstrumentStringData { Note = "G4", AudioName = "C_01_G4.wav", Description = "x2", Thickness = 3 },
            new InstrumentStringData { Note = "C5", AudioName = "C_02_C5.wav", Description = "x2", Thickness = 2 },
            new InstrumentStringData { Note = "E5", AudioName = "C_03_E5.wav", Description = "659.26 Hz", Thickness = 1 },
            new InstrumentStringData { Note = "E4", AudioName = "C_04_E4.wav", Description = "329.63 Hz", Thickness = 5 },
            new InstrumentStringData { Note = "A4", AudioName = "C_05_A4.wav", Description = "x2", Thickness = 4 },
            new InstrumentStringData { Note = "E5", AudioName = "C_06_E5.wav", Description = "x2", Thickness = 1 }
        };
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

    public void ClearAllBorders()
    {
        _borderAudioMap.Clear();
        _logger.LogInformation("Cleared all string borders");
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
