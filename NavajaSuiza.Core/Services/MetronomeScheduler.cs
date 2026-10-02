namespace NavajaSuiza.Core.Services;

public static class MetronomeScheduler
{
    public static (TimeSpan Delay, TimeSpan NextTick) GetTick(
        TimeSpan targetTick,
        TimeSpan elapsed,
        TimeSpan interval,
        TimeSpan? previousInterval = null)
    {
        if (previousInterval is { } previous && previous != interval)
            targetTick = elapsed + interval;

        var remaining = targetTick - elapsed;

        if (remaining > TimeSpan.Zero)
            return (remaining, targetTick + interval);

        if (remaining <= -interval)
            return (interval, elapsed + interval + interval);

        return (TimeSpan.Zero, targetTick + interval);
    }
}