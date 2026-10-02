namespace NavajaSuiza.Core.Services;

/// <summary>
/// Sintetiza el clic del metrónomo como muestras PCM. Es puro y sin dependencias de plataforma:
/// al no depender de archivos WAV, el metrónomo no puede quedar mudo por un recurso que no cargó.
/// </summary>
public static class MetronomeClick
{
    public const int ACCENT_FREQUENCY_HZ = 1000;
    public const int NORMAL_FREQUENCY_HZ = 800;
    public const int DURATION_MS = 50;

    public static int SampleCount(int durationMs, int sampleRate) =>
        (int)Math.Round(sampleRate * (durationMs / 1000.0));

    public static void Render(
        short[] destination,
        int destinationOffset,
        int sampleCount,
        int sampleRate,
        int frequencyHz,
        double decaySeconds)
    {
        for (var i = 0; i < sampleCount; i++)
        {
            var seconds = (double)i / sampleRate;
            var envelope = Math.Exp(-seconds / decaySeconds);
            var wave = Math.Sin(2.0 * Math.PI * frequencyHz * seconds) * envelope;

            destination[destinationOffset + i] = (short)(wave * short.MaxValue * 0.8);
        }
    }
}