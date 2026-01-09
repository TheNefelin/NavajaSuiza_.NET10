using CommunityToolkit.Maui.Views;
using NavajaSuiza_.NET10.Services.Interfaces;
using Plugin.Maui.Audio;
using Encoding = System.Text.Encoding;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class MetronomeService : IMetronomeService
{
    private System.Timers.Timer _timer;
    private int _currentBeat;
    private int _beatsPerMeasure;
    private MediaElement _accentMediaElement;
    private MediaElement _normalMediaElement;

    public void SetMediaElement(
        MediaElement accentMediaElement,
        MediaElement normalMediaElement)
    {
        _accentMediaElement = accentMediaElement;
        _normalMediaElement = normalMediaElement;

        _accentMediaElement.Source = MediaSource.FromResource("tik_50ms_1000hz.wav");
        _normalMediaElement.Source = MediaSource.FromResource("tik_50ms_800hz.wav");
    }

    public void Start(int currentBPM, string selectedTimeSignature)
    {
        Stop();

        if (_accentMediaElement == null || _normalMediaElement == null)
            throw new InvalidOperationException("MediaElement not set");

        // Validar BPM
        if (currentBPM < 50) currentBPM = 50;
        if (currentBPM > 350) currentBPM = 350;

        // Extraer beats del time signature
        _beatsPerMeasure = GetBeatsPerMeasure(selectedTimeSignature);
        _currentBeat = 1;

        // Calcular intervalo y iniciar timer
        double intervalMs = 60000.0 / currentBPM;
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += (s, e) => OnBeat();
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;

        try
        {
            _accentMediaElement?.Stop();
            _normalMediaElement?.Stop();
        }
        catch { }
    }

    private void OnBeat()
    {
        bool isAccent = _currentBeat == 1;
        //var mediaElement = isAccent ? _accentMediaElement : _normalMediaElement;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (isAccent)
                {
                    _accentMediaElement.SeekTo(TimeSpan.Zero);
                    _accentMediaElement.Play();
                    //_accentMediaElement.Stop();
                }
                else
                {
                    _normalMediaElement.SeekTo(TimeSpan.Zero);
                    _normalMediaElement.Play();
                    //_normalMediaElement.Stop();
                }
                //mediaElement.Stop();
                //mediaElement.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        });

        _currentBeat++;
        if (_currentBeat > _beatsPerMeasure)
            _currentBeat = 1;
    }

    private int GetBeatsPerMeasure(string timeSignature)
    {
        return timeSignature switch
        {
            "2/4" => 2,
            "3/4" => 3,
            "4/4" => 4,
            "5/4" => 5,
            "6/8" => 6,
            "7/8" => 7,
            _ => 4
        };
    }
}
