using CommunityToolkit.Maui.Views;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;
using System.Diagnostics;

namespace NavajaSuiza_.NET10.Services.Implementations;

/// <summary>
/// Motor del metrónomo para las plataformas sin <c>AudioTrack</c>: programa los latidos con
/// <see cref="MetronomeScheduler"/> sobre tiempos absolutos y los reproduce con <c>MediaElement</c>.
///
/// Es el único origen de audio de esta ruta, sin <c>SoundPool</c> ni fallback: si un clic no puede
/// reproducirse se omite, porque un clic tardío desincroniza más que un clic ausente.
/// </summary>
public sealed class MediaElementMetronomePlayer : IMetronomePlayer
{
    private const string ACCENT_CLIP = "tik_50ms_1000hz";
    private const string NORMAL_CLIP = "tik_50ms_800hz";
    private const int StopJoinTimeoutMs = 400;

    private readonly object _stateLock = new();

    private MediaElement? _accentMediaElement;
    private MediaElement? _normalMediaElement;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private double _intervalMs = MetronomeTempo.IntervalMs(AppConstants.Metronome.DEFAULT_BPM);
    private int _currentBeat = 1;
    private int _beatsPerMeasure = 4;
    private int _generation;

    public void AttachMediaSinks(object? accentMediaElement, object? normalMediaElement)
    {
        _accentMediaElement = accentMediaElement as MediaElement;
        _normalMediaElement = normalMediaElement as MediaElement;

        if (_accentMediaElement is not null)
            _accentMediaElement.Source = MediaSource.FromResource($"{ACCENT_CLIP}.wav");

        if (_normalMediaElement is not null)
            _normalMediaElement.Source = MediaSource.FromResource($"{NORMAL_CLIP}.wav");
    }

    public void Start(int bpm, int beatsPerMeasure)
    {
        Stop();

        lock (_stateLock)
        {
            _intervalMs = MetronomeTempo.IntervalMs(bpm);
            _beatsPerMeasure = Math.Max(1, beatsPerMeasure);
            _currentBeat = 1;
        }

        var cts = new CancellationTokenSource();
        var generation = Interlocked.Increment(ref _generation);

        _cts = cts;
        _loop = Task.Run(() => RunLoopAsync(cts.Token, generation));
    }

    public void SetTempo(int bpm)
    {
        lock (_stateLock)
            _intervalMs = MetronomeTempo.IntervalMs(bpm);
    }

    public void SetTimeSignature(int beatsPerMeasure)
    {
        lock (_stateLock)
        {
            _beatsPerMeasure = Math.Max(1, beatsPerMeasure);
            _currentBeat = 1;
        }
    }

    public void Stop()
    {
        var cts = Interlocked.Exchange(ref _cts, null);
        if (cts is null)
            return;

        cts.Cancel();

        var loop = Interlocked.Exchange(ref _loop, null);
        if (loop is not null)
        {
            try
            {
                loop.Wait(StopJoinTimeoutMs);
            }
            catch (AggregateException)
            {
            }
        }

        cts.Dispose();
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken, int generation)
    {
        var clock = Stopwatch.StartNew();
        var interval = ReadInterval();
        var lastInterval = interval;
        var nextTick = interval;

        try
        {
            while (!cancellationToken.IsCancellationRequested && Volatile.Read(ref _generation) == generation)
            {
                interval = ReadInterval();

                var tick = MetronomeScheduler.GetTick(nextTick, clock.Elapsed, interval, lastInterval);
                lastInterval = interval;
                nextTick = tick.NextTick;

                if (tick.Delay > TimeSpan.Zero)
                    await Task.Delay(tick.Delay, cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                    break;

                PlayBeat();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private TimeSpan ReadInterval()
    {
        lock (_stateLock)
            return TimeSpan.FromMilliseconds(_intervalMs);
    }

    private void PlayBeat()
    {
        bool isAccent;

        lock (_stateLock)
        {
            isAccent = _currentBeat == 1;
            _currentBeat = _currentBeat >= _beatsPerMeasure ? 1 : _currentBeat + 1;
        }

        var element = isAccent ? _accentMediaElement : _normalMediaElement;
        if (element is null)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                element.SeekTo(TimeSpan.Zero);
                element.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Metronome error: {ex.Message}");
            }
        });
    }
}