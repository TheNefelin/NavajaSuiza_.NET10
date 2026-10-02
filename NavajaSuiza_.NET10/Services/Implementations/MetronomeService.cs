using CommunityToolkit.Maui.Views;
using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;
using System.Diagnostics;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class MetronomeService : IMetronomeService, IDisposable
{
    private const string ACCENT_CLIP = "tik_50ms_1000hz";
    private const string NORMAL_CLIP = "tik_50ms_800hz";
    private const string BPM_KEY = "metronome_bpm";
    private const string TIME_SIGNATURE_KEY = "metronome_time_signature";

    private readonly object _stateLock = new();
    private CancellationTokenSource? _cts;
    private Task? _timerTask;
    private double _intervalMs = 500;
    private int _currentBeat = 1;
    private int _beatsPerMeasure = 4;
    private MediaElement? _accentMediaElement;
    private MediaElement? _normalMediaElement;

#if ANDROID
    private Android.Media.SoundPool? _soundPool;
    private int _accentSoundId;
    private int _normalSoundId;
#endif

    public void SetMediaElement(object accentMediaElement, object normalMediaElement)
    {
        _accentMediaElement = accentMediaElement as MediaElement;
        _normalMediaElement = normalMediaElement as MediaElement;

#if ANDROID
        if (_accentMediaElement != null)
            _accentMediaElement.Source = MediaSource.FromResource($"{ACCENT_CLIP}.wav");
        if (_normalMediaElement != null)
            _normalMediaElement.Source = MediaSource.FromResource($"{NORMAL_CLIP}.wav");

        InitializeSoundPool();
#endif
    }

    public async Task StartAsync(int currentBPM, string selectedTimeSignature)
    {
        await StopAsync().ConfigureAwait(false);

        if (currentBPM < 50) currentBPM = 50;
        if (currentBPM > 350) currentBPM = 350;

        lock (_stateLock)
        {
            _beatsPerMeasure = GetBeatsPerMeasure(selectedTimeSignature);
            _currentBeat = 1;
            _intervalMs = 60000.0 / currentBPM;
        }

        _cts = new CancellationTokenSource();
        _timerTask = RunTimerAsync(_cts.Token);
    }

    public int SavedBpm => Preferences.Default.Get(BPM_KEY, AppConstants.Metronome.DEFAULT_BPM);

    public string SavedTimeSignature => Preferences.Default.Get(TIME_SIGNATURE_KEY, AppConstants.Metronome.DEFAULT_TIME_SIGNATURE);

    public void SetTempo(int currentBPM)
    {
        if (currentBPM < 50) currentBPM = 50;
        if (currentBPM > 350) currentBPM = 350;

        lock (_stateLock)
            _intervalMs = 60000.0 / currentBPM;

        Preferences.Default.Set(BPM_KEY, currentBPM);
    }

    public void SetTimeSignature(string selectedTimeSignature)
    {
        lock (_stateLock)
        {
            _beatsPerMeasure = GetBeatsPerMeasure(selectedTimeSignature);
            _currentBeat = 1;
        }

        Preferences.Default.Set(TIME_SIGNATURE_KEY, selectedTimeSignature);
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();

        try
        {
            _accentMediaElement?.Stop();
            _normalMediaElement?.Stop();
        }
        catch { }

        if (_timerTask != null)
        {
            try
            {
                await _timerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }

            _timerTask = null;
        }

        _cts?.Dispose();
        _cts = null;
    }

    private async Task RunTimerAsync(CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        TimeSpan interval;
        lock (_stateLock)
            interval = TimeSpan.FromMilliseconds(_intervalMs);

        var nextTick = interval;
        var lastInterval = interval;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                lock (_stateLock)
                    interval = TimeSpan.FromMilliseconds(_intervalMs);

                var tick = MetronomeScheduler.GetTick(nextTick, stopwatch.Elapsed, interval, lastInterval);
                lastInterval = interval;
                nextTick = tick.NextTick;

                if (tick.Delay > TimeSpan.Zero)
                    await Task.Delay(tick.Delay, ct).ConfigureAwait(false);

                if (ct.IsCancellationRequested)
                    break;

                PlayBeat();
            }
        }
        catch (OperationCanceledException) { }
    }

    private void PlayBeat()
    {
        bool isAccent;
        lock (_stateLock)
        {
            isAccent = _currentBeat == 1;
            _currentBeat = _currentBeat >= _beatsPerMeasure ? 1 : _currentBeat + 1;
        }

#if ANDROID
        if (_soundPool != null)
        {
            var soundId = isAccent ? _accentSoundId : _normalSoundId;

            if (soundId != 0 && _soundPool.Play(soundId, 1f, 1f, 1, 0, 1f) != 0)
                return;
        }
#endif

        PlayWithMediaElement(isAccent);
    }

    private void PlayWithMediaElement(bool isAccent)
    {
        var element = isAccent ? _accentMediaElement : _normalMediaElement;
        if (element == null)
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
                System.Diagnostics.Debug.WriteLine($"Metronome error: {ex.Message}");
            }
        });
    }

    private int GetBeatsPerMeasure(string timeSignature)
    {
        return timeSignature switch
        {
            "2/2" => 2,
            "2/4" => 2,
            "3/4" => 3,
            "4/4" => 4,
            "5/4" => 5,
            "6/8" => 6,
            "7/8" => 7,
            _ => 4
        };
    }

#if ANDROID
    private void InitializeSoundPool()
    {
        if (_soundPool != null)
            return;

        var context = Android.App.Application.Context;

        var audioAttributes = new Android.Media.AudioAttributes.Builder()
            .SetUsage(Android.Media.AudioUsageKind.Game)!
            .SetContentType(Android.Media.AudioContentType.Sonification)!
            .Build();

        _soundPool = new Android.Media.SoundPool.Builder()
            .SetMaxStreams(2)!
            .SetAudioAttributes(audioAttributes)!
            .Build();

        if (_soundPool == null)
        {
            System.Diagnostics.Debug.WriteLine("Metronome SoundPool not created");
            return;
        }

        _accentSoundId = LoadClip(context, ACCENT_CLIP);
        _normalSoundId = LoadClip(context, NORMAL_CLIP);
    }

    private int LoadClip(Android.Content.Context context, string clipName)
    {
        var resources = context.Resources;
        if (resources == null)
            return 0;

        var resourceId = resources.GetIdentifier(clipName, "raw", context.PackageName);
        if (resourceId == 0)
        {
            System.Diagnostics.Debug.WriteLine($"Metronome clip not found: {clipName}");
            return 0;
        }

        return _soundPool!.Load(context, resourceId, 1);
    }
#endif

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

#if ANDROID
        _soundPool?.Release();
        _soundPool?.Dispose();
        _soundPool = null;
#endif

        GC.SuppressFinalize(this);
    }
}
