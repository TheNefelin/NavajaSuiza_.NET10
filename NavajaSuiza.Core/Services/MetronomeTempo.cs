namespace NavajaSuiza.Core.Services;

/// <summary>
/// Reglas de tempo del metrónomo en un solo lugar, puras y testeables: rango válido, intervalo y
/// traducción de firma de tiempos a pulsos por compás.
/// </summary>
public static class MetronomeTempo
{
    public const int MIN_BPM = 50;
    public const int MAX_BPM = 350;

    public static int Clamp(int bpm) => Math.Clamp(bpm, MIN_BPM, MAX_BPM);

    public static double IntervalMs(int bpm) => 60000.0 / Clamp(bpm);

    public static double SamplesPerBeat(int sampleRate, int bpm) => sampleRate * 60.0 / Clamp(bpm);

    public static int BeatsPerMeasure(string timeSignature) => timeSignature switch
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