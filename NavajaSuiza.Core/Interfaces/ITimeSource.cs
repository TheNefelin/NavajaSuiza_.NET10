namespace NavajaSuiza.Core.Interfaces;

/// <summary>
/// Fuente de tiempo para medir intervalos. Debe avanzar aunque el
/// dispositivo duerma (deep sleep) para evitar pérdidas de tiempo al
/// bloquear la pantalla. Solo se usa por diferencias entre lecturas.
/// </summary>
public interface ITimeSource
{
    DateTime UtcNow { get; }
}