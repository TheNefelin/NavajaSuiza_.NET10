using System.Diagnostics;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Services;

public class StopwatchService : IStopwatchService
{
    private readonly object _lock = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly List<StopwatchLap> _laps = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public event Action<TimeSpan>? Tick;

    public bool IsRunning
    {
        get { lock (_lock) { return _isRunning; } }
    }

    public TimeSpan Elapsed
    {
        get { lock (_lock) { return _stopwatch.Elapsed; } }
    }

    public IReadOnlyList<StopwatchLap> Laps
    {
        get { lock (_lock) { return _laps.ToArray(); } }
    }

    public void AddLap(StopwatchLap lap)
    {
        lock (_lock)
        {
            _laps.Insert(0, lap);
        }
    }

    public void ClearLaps()
    {
        lock (_lock)
        {
            _laps.Clear();
        }
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _stopwatch.Start();
        }

        _cts ??= new CancellationTokenSource();
        _ = RunTickerAsync(_cts.Token);
    }

    public void Pause()
    {
        lock (_lock)
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _stopwatch.Stop();
        }

        CancelTicker();
    }

    public void Stop()
    {
        CancelTicker();

        lock (_lock)
        {
            _isRunning = false;
            _stopwatch.Reset();
        }

        Tick?.Invoke(TimeSpan.Zero);
    }

    private void CancelTicker()
    {
        var cts = _cts;
        _cts = null;
        cts?.Cancel();
        cts?.Dispose();
    }

    private async Task RunTickerAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AppConstants.Stopwatch.TICK_INTERVAL_MS));

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (ct.IsCancellationRequested)
                    return;

                TimeSpan elapsed;
                lock (_lock) { elapsed = _stopwatch.Elapsed; }
                Tick?.Invoke(elapsed);
            }
        }
        catch (OperationCanceledException) { }
    }
}