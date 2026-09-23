#if ANDROID
using Android.OS;
#endif
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

/// <summary>
/// Fuente de tiempo para el cronómetro que NO pierde el tiempo transcurrido
/// durante el deep sleep del dispositivo.
///
/// En Android usa <see cref="SystemClock.ElapsedRealtimeNanos"/>: reloj
/// monotónico que incluye el tiempo en deep sleep (a diferencia de
/// <see cref="System.Diagnostics.Stopwatch"/>, basado en CLOCK_MONOTONIC,
/// que se congela durante el sueño profundo). En las demás plataformas
/// usa <see cref="DateTime.UtcNow"/> (reloj de pared, portable).
/// </summary>
public sealed class RealtimeTimeSource : ITimeSource
{
    public DateTime UtcNow
    {
        get
        {
#if ANDROID
            return DateTime.UnixEpoch.AddTicks(SystemClock.ElapsedRealtimeNanos() / 100);
#else
            return DateTime.UtcNow;
#endif
        }
    }
}
