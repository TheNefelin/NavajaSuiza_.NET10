namespace NavajaSuiza.Core.Interfaces;

/// <summary>
/// Motor de reproducción del metrónomo. Cada plataforma aporta una implementación y solo una queda
/// activa a la vez: así es imposible que dos fuentes de audio disparen el mismo latido.
/// </summary>
public interface IMetronomePlayer : IDisposable
{
    /// <summary>Comienza la reproducción. Si ya hay una sesión activa, la reemplaza.</summary>
    void Start(int bpm, int beatsPerMeasure);

    void SetTempo(int bpm);

    void SetTimeSignature(int beatsPerMeasure);

    void Stop();

    /// <summary>
    /// Entrega los <c>MediaElement</c> de la página. Solo lo usan los players que reproducen un
    /// archivo; los que sintetizan el clic lo ignoran.
    /// </summary>
    void AttachMediaSinks(object? accentMediaElement, object? normalMediaElement);
}