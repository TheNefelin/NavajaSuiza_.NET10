namespace NavajaSuiza.Core.Services;

/// <summary>Posición de un clic dentro del buffer que se está por escribir.</summary>
public readonly record struct MetronomeBeat(int OffsetInBuffer, int SampleCount, bool IsAccent);

/// <summary>
/// Calcula en qué muestra del stream debe caer cada latido. El tempo se deriva del reloj del audio
/// (cuántas muestras han salido) y no de un timer de sistema: por eso no acumula deriva ni jitter
/// del scheduler, que es justo lo que un metrónomo no puede tolerar.
/// </summary>
public sealed class MetronomeSampleClock
{
    private const int MaxBeatsPerBuffer = 512;

    private readonly int _sampleRate;
    private double _samplesPerBeat;
    private double _accumulator;
    private long _nextBeatSample;
    private int _beatInMeasure;
    private int _beatsPerMeasure;

    public MetronomeSampleClock(int sampleRate, int bpm, int beatsPerMeasure)
    {
        _sampleRate = sampleRate;

        SetTempo(bpm);
        SetTimeSignature(beatsPerMeasure);

        _accumulator = _samplesPerBeat;
        _nextBeatSample = (long)_accumulator;
    }

    public int SampleRate => _sampleRate;

    public int Bpm { get; private set; }

    public int BeatsPerMeasure => _beatsPerMeasure;

    public double SamplesPerBeat => _samplesPerBeat;

    public long NextBeatSample => _nextBeatSample;

    /// <summary>
    /// Cambia el tempo conservando la fase actual, para que el cambio sea fluido en vez de reiniciar
    /// el compás.
    /// </summary>
    public void SetTempo(int bpm)
    {
        Bpm = MetronomeTempo.Clamp(bpm);
        _samplesPerBeat = MetronomeTempo.SamplesPerBeat(_sampleRate, Bpm);
        _accumulator = _nextBeatSample;
    }

    public void SetTimeSignature(int beatsPerMeasure)
    {
        _beatsPerMeasure = Math.Max(1, beatsPerMeasure);
        _beatInMeasure = 1;
    }

    /// <summary>
    /// Devuelve los latidos que caen dentro del buffer que empieza en
    /// <paramref name="bufferStartSample"/> y avanza el reloj.
    /// </summary>
    public IReadOnlyList<MetronomeBeat> ConsumeBeats(long bufferStartSample, int bufferLengthSamples)
    {
        var bufferEndSample = bufferStartSample + bufferLengthSamples;
        List<MetronomeBeat>? beats = null;
        var guard = 0;

        while (_nextBeatSample < bufferEndSample && guard++ < MaxBeatsPerBuffer)
        {
            var isAccent = _beatInMeasure == 1;

            if (_nextBeatSample >= bufferStartSample)
            {
                beats ??= new List<MetronomeBeat>();
                beats.Add(new MetronomeBeat(
                    (int)(_nextBeatSample - bufferStartSample),
                    MetronomeClick.SampleCount(MetronomeClick.DURATION_MS, _sampleRate),
                    isAccent));
            }

            _beatInMeasure = _beatInMeasure >= _beatsPerMeasure ? 1 : _beatInMeasure + 1;
            _accumulator += _samplesPerBeat;
            _nextBeatSample = (long)_accumulator;
        }

        return (IReadOnlyList<MetronomeBeat>?)beats ?? Array.Empty<MetronomeBeat>();
    }
}