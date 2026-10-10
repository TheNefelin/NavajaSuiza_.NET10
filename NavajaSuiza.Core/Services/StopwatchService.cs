using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Services;

public class StopwatchService : IStopwatchService
{
    private readonly object _lock = new();
    private readonly ITimeSource _timeSource;
    private readonly List<StopwatchLap> _laps = new();
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private DateTime _startedAt;
    private TimeSpan _accumulated;

    public event Action<TimeSpan>? Tick;

    public bool IsRunning
    {
        get { lock (_lock) { return _isRunning; } }
    }

    public TimeSpan Elapsed
    {
        get
        {
            lock (_lock)
            {
                return _accumulated + (_isRunning ? _timeSource.UtcNow - _startedAt : TimeSpan.Zero);
            }
        }
    }

    public IReadOnlyList<StopwatchLap> Laps
    {
        get { lock (_lock) { return _laps.ToArray(); } }
    }

    public StopwatchService(ITimeSource timeSource)
    {
        _timeSource = timeSource;
    }

    public void AddLap(StopwatchLap lap)
    {
        lock (_lock)
        {
            _laps.Insert(0, lap);
        }
    }

    public void RemoveLap(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _laps.Count)
                return;

            _laps.RemoveAt(index);
            RenumberLaps();
        }
    }

    private void RenumberLaps()
    {
        for (var i = 0; i < _laps.Count; i++)
        {
            var previousSplit = i + 1 < _laps.Count ? _laps[i + 1].Split : TimeSpan.Zero;
            var number = _laps.Count - i;

            if (_laps[i].Number != number)
            {
                _laps[i] = new StopwatchLap
                {
                    Number = number,
                    Split = _laps[i].Split,
                    Delta = _laps[i].Split - previousSplit
                };
            }
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
        CancellationTokenSource cts;
        lock (_lock)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _startedAt = _timeSource.UtcNow;
            cts = _cts ??= new CancellationTokenSource();
        }

        _ = RunTickerAsync(cts);
    }

    public void Pause()
    {
        lock (_lock)
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _accumulated += _timeSource.UtcNow - _startedAt;
        }

        CancelTicker();
    }

    public void Stop()
    {
        CancelTicker();

        lock (_lock)
        {
            _isRunning = false;
            _accumulated = TimeSpan.Zero;
        }

        Tick?.Invoke(TimeSpan.Zero);
    }

    private void CancelTicker()
    {
        CancellationTokenSource? cts;
        lock (_lock)
        {
            cts = _cts;
            _cts = null;
        }

        // No se libera aquí: el ticker aún puede estar usando el token.
        // RunTickerAsync lo libera en su finally, una vez que terminó.
        cts?.Cancel();
    }

    private async Task RunTickerAsync(CancellationTokenSource cts)
    {
        var ct = cts.Token;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(AppConstants.Stopwatch.TICK_INTERVAL_MS));

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                if (ct.IsCancellationRequested)
                    return;

                TimeSpan elapsed;
                lock (_lock) { elapsed = Elapsed; }
                Tick?.Invoke(elapsed);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            cts.Dispose();
        }
    }
}