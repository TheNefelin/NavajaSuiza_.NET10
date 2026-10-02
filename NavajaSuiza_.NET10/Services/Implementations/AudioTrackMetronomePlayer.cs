#if ANDROID
using Android.Media;
using Android.OS;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;
using AudioStream = Android.Media.Stream;
using Process = Android.OS.Process;

namespace NavajaSuiza_.NET10.Services.Implementations;

/// <summary>
/// Motor del metrónomo para Android basado en <see cref="AudioTrack"/> en modo streaming.
///
/// En lugar de disparar un clic por latido desde un timer, mantiene un stream continuo y coloca las
/// muestras del clic en la posición exacta que le indica <see cref="MetronomeSampleClock"/>. El tempo
/// queda derivado del reloj de audio (muestras reproduce el stream), no de <c>Task.Delay</c>, que es
/// la razón por la que <c>SoundPool</c> más un timer no puede sostener la precisión de un metrónomo.
///
/// No hay ruta alternativa de audio: un solo origen por latido, sin fallback que pueda solaparse.
/// </summary>
public sealed class AudioTrackMetronomePlayer : IMetronomePlayer
{
    private const int FallbackSampleRate = 48000;
    private const int BytesPerSample = 2;
    private const int StopJoinTimeoutMs = 400;

    private readonly object _clockLock = new();

    private AudioTrack? _track;
    private MetronomeSampleClock? _clock;
    private Thread? _renderThread;
    private short[] _accentClick = [];
    private short[] _normalClick = [];
    private short[] _pendingTail = [];
    private int _pendingLength;
    private int _bufferFrames;
    private int _sampleRate = FallbackSampleRate;
    private long _writePosition;
    private volatile bool _isRunning;
    private int _generation;

    public void Start(int bpm, int beatsPerMeasure)
    {
        Stop();

        _sampleRate = ResolveNativeSampleRate();
        _bufferFrames = ResolveBufferFrames(_sampleRate);

        _accentClick = RenderClick(MetronomeClick.ACCENT_FREQUENCY_HZ);
        _normalClick = RenderClick(MetronomeClick.NORMAL_FREQUENCY_HZ);
        _pendingTail = new short[_accentClick.Length];
        _pendingLength = 0;
        _writePosition = 0;

        _clock = new MetronomeSampleClock(_sampleRate, bpm, beatsPerMeasure);

        if (_track is null || _track.State != AudioTrackState.Initialized)
        {
            _track?.Dispose();
            _track = CreateTrack();
        }

        _track.Play();

        var generation = Interlocked.Increment(ref _generation);
        _isRunning = true;

        _renderThread = new Thread(() => RenderLoop(generation))
        {
            IsBackground = true,
            Name = "MetronomeRender"
        };

        _renderThread.Start();
    }

    public void SetTempo(int bpm)
    {
        lock (_clockLock)
            _clock?.SetTempo(bpm);
    }

    public void SetTimeSignature(int beatsPerMeasure)
    {
        lock (_clockLock)
            _clock?.SetTimeSignature(beatsPerMeasure);
    }

    public void Stop()
    {
        _isRunning = false;

        var track = _track;
        if (track is not null)
        {
            try
            {
                if (track.State == AudioTrackState.Initialized)
                    track.Stop();
            }
            catch (Exception)
            {
            }
        }

        var thread = Interlocked.Exchange(ref _renderThread, null);
        if (thread is not null && thread.IsAlive && !ReferenceEquals(thread, Thread.CurrentThread))
            thread.Join(StopJoinTimeoutMs);
    }

    public void AttachMediaSinks(object? accentMediaElement, object? normalMediaElement)
    {
    }

    public void Dispose()
    {
        Stop();

        _track?.Dispose();
        _track = null;
    }

    private void RenderLoop(int generation)
    {
        Process.SetThreadPriority(Android.OS.ThreadPriority.UrgentAudio);

        var buffer = new short[_bufferFrames];

        while (_isRunning && Volatile.Read(ref _generation) == generation)
        {
            lock (_clockLock)
                FillBuffer(buffer);

            var track = _track;
            if (track is null || track.State != AudioTrackState.Initialized)
                break;

            if (track.Write(buffer, 0, buffer.Length) < 0)
                break;
        }
    }

    /// <summary>
    /// Rellena el buffer con los clics que caen dentro de él. Un clic que se parte contra el borde
    /// del buffer arrastra su cola al siguiente: sin esto el corte se oye como un chasquido seco.
    /// </summary>
    private void FillBuffer(short[] buffer)
    {
        Array.Clear(buffer);

        if (_pendingLength > 0)
        {
            Accumulate(buffer, 0, _pendingTail, _pendingLength);
            _pendingLength = 0;
        }

        var clock = _clock;
        if (clock is null)
            return;

        var beats = clock.ConsumeBeats(_writePosition, buffer.Length);

        foreach (var beat in beats)
            PlaceClick(buffer, beat.OffsetInBuffer, beat.IsAccent ? _accentClick : _normalClick);

        _writePosition += buffer.Length;
    }

    private void PlaceClick(short[] buffer, int offsetInBuffer, short[] click)
    {
        var room = buffer.Length - offsetInBuffer;
        var toCopy = Math.Min(click.Length, room);

        Accumulate(buffer, offsetInBuffer, click, toCopy);

        var remainder = click.Length - toCopy;
        if (remainder <= 0)
            return;

        Array.Copy(click, toCopy, _pendingTail, 0, remainder);
        _pendingLength = remainder;
    }

    /// <summary>
    /// Mezcla el clic por suma y no por copia: la cola de un clic y el latido siguiente pueden caer en
    /// el mismo buffer, y sobrescribir uno con otro recortaría la envolvente en ese latido.
    /// </summary>
    private static void Accumulate(short[] buffer, int offsetInBuffer, short[] source, int count)
    {
        for (var i = 0; i < count; i++)
            buffer[offsetInBuffer + i] = (short)Math.Clamp(buffer[offsetInBuffer + i] + source[i], short.MinValue, short.MaxValue);
    }

    private short[] RenderClick(int frequencyHz)
    {
        var sampleCount = MetronomeClick.SampleCount(MetronomeClick.DURATION_MS, _sampleRate);
        var click = new short[sampleCount];

        MetronomeClick.Render(click, 0, sampleCount, _sampleRate, frequencyHz, 0.008);

        return click;
    }

    private AudioTrack CreateTrack()
    {
        var track = new AudioTrack(
            AudioStream.Music,
            _sampleRate,
            ChannelOut.Mono,
            Encoding.Pcm16bit,
            _bufferFrames * BytesPerSample,
            AudioTrackMode.Stream);

        if (track.State != AudioTrackState.Initialized)
            throw new InvalidOperationException("No se pudo inicializar el AudioTrack del metrónomo.");

        return track;
    }

    private static int ResolveBufferFrames(int sampleRate)
    {
        var minimum = AudioTrack.GetMinBufferSize(sampleRate, ChannelOut.Mono, Encoding.Pcm16bit);

        return minimum > 0
            ? minimum / BytesPerSample
            : sampleRate / 20;
    }

    private static int ResolveNativeSampleRate()
    {
        try
        {
            var nativeRate = AudioTrack.GetNativeOutputSampleRate(AudioStream.Music);

            return nativeRate > 0 ? nativeRate : FallbackSampleRate;
        }
        catch (Exception)
        {
            return FallbackSampleRate;
        }
    }
}
#endif