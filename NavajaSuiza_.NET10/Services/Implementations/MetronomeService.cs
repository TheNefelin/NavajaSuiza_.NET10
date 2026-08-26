using CommunityToolkit.Maui.Views;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class MetronomeService : IMetronomeService
{
    private CancellationTokenSource? _cts;
    private int _currentBeat;
    private int _beatsPerMeasure;
    private MediaElement? _accentMediaElement;
    private MediaElement? _normalMediaElement;

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

        if (currentBPM < 50) currentBPM = 50;
        if (currentBPM > 350) currentBPM = 350;

        _beatsPerMeasure = GetBeatsPerMeasure(selectedTimeSignature);
        _currentBeat = 1;

        double intervalMs = 60000.0 / currentBPM;
        _cts = new CancellationTokenSource();
        _ = RunTimerAsync(intervalMs, _cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        try
        {
            _accentMediaElement?.Stop();
            _normalMediaElement?.Stop();
        }
        catch { }
    }

    private async Task RunTimerAsync(double intervalMs, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                await PlayBeatAsync();
            }
        }
        catch (OperationCanceledException) { }
    }

    private Task PlayBeatAsync()
    {
        var tcs = new TaskCompletionSource();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (_currentBeat == 1)
                {
                    _accentMediaElement!.SeekTo(TimeSpan.Zero);
                    _accentMediaElement.Play();
                }
                else
                {
                    _normalMediaElement!.SeekTo(TimeSpan.Zero);
                    _normalMediaElement.Play();
                }

                tcs.SetResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Metronome error: {ex.Message}");
                tcs.SetResult();
            }
        });

        _currentBeat++;
        if (_currentBeat > _beatsPerMeasure)
            _currentBeat = 1;

        return tcs.Task;
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
