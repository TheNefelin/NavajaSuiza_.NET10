#if ANDROID
using Android.Media;
using Android.OS;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AudioTrackMetronomePlayer> _logger;

    private AudioTrack? _track;
    private MetronomeSampleClock? _clock;
    private Session? _session;
    private Thread? _renderThread;

    public AudioTrackMetronomePlayer(ILogger<AudioTrackMetronomePlayer> logger)
    {
        _logger = logger;
    }

    public void Start(int bpm, int beatsPerMeasure)
    {
        Stop();

        _logger.LogInformation("Metrónomo: inicio solicitado ({Bpm} BPM, {Beats} pulsos)", bpm, beatsPerMeasure);

        Session session;
        try
        {
            var sampleRate = ResolveNativeSampleRate();
            var bufferFrames = Math.Max(
                ResolveBufferFrames(sampleRate),
                MetronomeClick.SampleCount(MetronomeClick.DURATION_MS, sampleRate));

            var accentClick = RenderClick(sampleRate, MetronomeClick.ACCENT_FREQUENCY_HZ);
            var normalClick = RenderClick(sampleRate, MetronomeClick.NORMAL_FREQUENCY_HZ);

            var track = GetOrCreateTrack(sampleRate, bufferFrames);

            session = new Session(track, new MetronomeSampleClock(sampleRate, bpm, beatsPerMeasure), bufferFrames, accentClick, normalClick);

            lock (_clockLock)
                _clock = session.Clock;

            _logger.LogInformation(
                "Metrónomo: AudioTrack inicializado ({SampleRate} Hz, buffer {BufferFrames} frames)",
                sampleRate,
                bufferFrames);

            track.Play();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrónomo: fallo al iniciar el AudioTrack");
            throw;
        }

        _session = session;

        _renderThread = new Thread(() => RenderLoop(session))
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
        Interlocked.Exchange(ref _session, null);

        lock (_clockLock)
            _clock = null;

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
        if (thread is not null && thread.IsAlive && !ReferenceEquals(thread, Thread.CurrentThread) && !thread.Join(StopJoinTimeoutMs))
        {
            // El hilo viejo no murió: se suelta el track para que Start cree uno nuevo y el hilo
            // viejo libere el suyo al salir. Evita dos escritores sobre el mismo AudioTrack.
            Interlocked.CompareExchange(ref _track, null, track);
        }
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

    private void RenderLoop(Session session)
    {
        try
        {
            Process.SetThreadPriority(Android.OS.ThreadPriority.UrgentAudio);

            while (ReferenceEquals(Volatile.Read(ref _session), session))
            {
                FillBuffer(session);

                var track = session.Track;
                if (track.State != AudioTrackState.Initialized)
                    break;

                if (track.Write(session.Buffer, 0, session.Buffer.Length) < 0)
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Metrónomo: fallo en el hilo de render de audio");
            Interlocked.CompareExchange(ref _session, null, session);
        }
        finally
        {
            if (!ReferenceEquals(session.Track, Volatile.Read(ref _track)))
                session.Track.Dispose();
        }
    }

    /// <summary>
    /// Rellena el buffer de la sesión con los clics que caen dentro de él. Un clic que se parte contra
    /// el borde del buffer arrastra su cola al siguiente: sin esto el corte se oye como un chasquido seco.
    /// </summary>
    private void FillBuffer(Session session)
    {
        var buffer = session.Buffer;
        Array.Clear(buffer);

        if (session.PendingLength > 0)
        {
            Accumulate(buffer, 0, session.PendingTail, session.PendingLength);
            session.PendingLength = 0;
        }

        IReadOnlyList<MetronomeBeat> beats;
        lock (_clockLock)
            beats = session.Clock.ConsumeBeats(session.WritePosition, buffer.Length);

        foreach (var beat in beats)
            PlaceClick(session, beat.OffsetInBuffer, beat.IsAccent ? session.AccentClick : session.NormalClick);

        session.WritePosition += buffer.Length;
    }

    private static void PlaceClick(Session session, int offsetInBuffer, short[] click)
    {
        var buffer = session.Buffer;
        var room = buffer.Length - offsetInBuffer;
        var toCopy = Math.Min(click.Length, room);

        Accumulate(buffer, offsetInBuffer, click, toCopy);

        var remainder = click.Length - toCopy;
        if (remainder <= 0)
            return;

        Array.Copy(click, toCopy, session.PendingTail, 0, remainder);
        session.PendingLength = remainder;
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

    private static short[] RenderClick(int sampleRate, int frequencyHz)
    {
        var sampleCount = MetronomeClick.SampleCount(MetronomeClick.DURATION_MS, sampleRate);
        var click = new short[sampleCount];

        MetronomeClick.Render(click, 0, sampleCount, sampleRate, frequencyHz, 0.008);

        return click;
    }

    private AudioTrack GetOrCreateTrack(int sampleRate, int bufferFrames)
    {
        if (_track is not null && _track.State == AudioTrackState.Initialized)
            return _track;

        _track?.Dispose();
        _track = CreateTrack(sampleRate, bufferFrames);
        return _track;
    }

    private static AudioTrack CreateTrack(int sampleRate, int bufferFrames)
    {
        var track = new AudioTrack(
            AudioStream.Music,
            sampleRate,
            ChannelOut.Mono,
            Encoding.Pcm16bit,
            bufferFrames * BytesPerSample,
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

    /// <summary>
    /// Estado privado de una sesión de reproducción. El hilo de render solo lee y escribe su propia
    /// sesión; el único objeto compartido con el resto de la clase es <see cref="Clock"/> (cambio en
    /// caliente de tempo/compás), protegido por <c>_clockLock</c>.
    /// </summary>
    private sealed class Session
    {
        public Session(AudioTrack track, MetronomeSampleClock clock, int bufferFrames, short[] accentClick, short[] normalClick)
        {
            Track = track;
            Clock = clock;
            Buffer = new short[bufferFrames];
            AccentClick = accentClick;
            NormalClick = normalClick;
            PendingTail = new short[accentClick.Length];
        }

        public AudioTrack Track { get; }
        public MetronomeSampleClock Clock { get; }
        public short[] Buffer { get; }
        public short[] AccentClick { get; }
        public short[] NormalClick { get; }
        public short[] PendingTail { get; }
        public int PendingLength { get; set; }
        public long WritePosition { get; set; }
    }
}
#endif
