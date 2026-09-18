namespace NavajaSuiza.Core.Interfaces;

public interface IStopwatchService
{
    event Action<TimeSpan>? Tick;
    bool IsRunning { get; }
    TimeSpan Elapsed { get; }
    void Start();
    void Pause();
    void Stop();
}