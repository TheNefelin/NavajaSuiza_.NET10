namespace NavajaSuiza.Core.Models;

public class StopwatchLap
{
    public int Number { get; init; }
    public TimeSpan Split { get; init; }
    public TimeSpan Delta { get; init; }

    public string SplitText => FormatTime(Split);

    public string DeltaText => $"+{FormatTime(Delta)}";

    public static string FormatTime(TimeSpan value)
    {
        return $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}.{value.Milliseconds:000}";
    }
}