using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface IStopwatchService
{
    event Action<TimeSpan>? Tick;
    bool IsRunning { get; }
    TimeSpan Elapsed { get; }
    IReadOnlyList<StopwatchLap> Laps { get; }
    void Start();
    void Pause();
    void Stop();
    void AddLap(StopwatchLap lap);
    void ClearLaps();
}