using CommunityToolkit.Maui.Views;
using NavajaSuiza.Core.Interfaces;
using System.Diagnostics;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class MetronomeService : IMetronomeService
{
    private const string ACCENT_CLIP = "tik_50ms_1000hz";
    private const string NORMAL_CLIP = "tik_50ms_800hz";

    private CancellationTokenSource? _cts;
    private int _currentBeat;
    private int _beatsPerMeasure;
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
        InitializeSoundPool();
#else
        if (_accentMediaElement != null)
            _accentMediaElement.Source = MediaSource.FromResource($"{ACCENT_CLIP}.wav");
        if (_normalMediaElement != null)
            _normalMediaElement.Source = MediaSource.FromResource($"{NORMAL_CLIP}.wav");
#endif
    }

    public void Start(int currentBPM, string selectedTimeSignature)
    {
        Stop();

        if (currentBPM < 50) currentBPM = 50;
        if (currentBPM > 350) currentBPM = 350;

        _beatsPerMeasure = GetBeatsPerMeasure(selectedTimeSignature);
        _currentBeat = 1;

        double intervalMs = 60000.0 / currentBPM;
        _cts = new CancellationTokenSource();
        _ = RunTimerAsync(TimeSpan.FromMilliseconds(intervalMs), _cts.Token);
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

    private async Task RunTimerAsync(TimeSpan interval, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var nextTick = interval;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var remaining = nextTick - stopwatch.Elapsed;
                if (remaining > TimeSpan.Zero)
                    await Task.Delay(remaining, ct).ConfigureAwait(false);

                if (ct.IsCancellationRequested)
                    break;

                PlayBeat();

                nextTick += interval;
            }
        }
        catch (OperationCanceledException) { }
    }

    private void PlayBeat()
    {
        var isAccent = _currentBeat == 1;
        _currentBeat++;
        if (_currentBeat > _beatsPerMeasure)
            _currentBeat = 1;

#if ANDROID
        if (_soundPool != null)
        {
            var soundId = isAccent ? _accentSoundId : _normalSoundId;

            if (soundId != 0)
            {
                _soundPool.Play(soundId, 1f, 1f, 1, 0, 1f);
                return;
            }
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

        _soundPool = new Android.Media.SoundPool.Builder()
            .SetMaxStreams(2)
            .SetAudioAttributes(new Android.Media.AudioAttributes.Builder()
                .SetUsage(Android.Media.AudioUsageKind.Game)
                .SetContentType(Android.Media.AudioContentType.Sonification)
                .Build())
            .Build();

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
}
