using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class StopwatchServiceTests
{
    [Fact]
    public void IsRunning_DefaultsToFalse()
    {
        var service = new StopwatchService();
        Assert.False(service.IsRunning);
    }

    [Fact]
    public void Elapsed_DefaultsToZero()
    {
        var service = new StopwatchService();
        Assert.Equal(TimeSpan.Zero, service.Elapsed);
    }

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var service = new StopwatchService();
        service.Start();
        Assert.True(service.IsRunning);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotThrow()
    {
        var service = new StopwatchService();
        service.Start();
        service.Start();
        Assert.True(service.IsRunning);
    }

    [Fact]
    public void Pause_WhenRunning_SetsIsRunningFalse()
    {
        var service = new StopwatchService();
        service.Start();
        service.Pause();
        Assert.False(service.IsRunning);
    }

    [Fact]
    public void Pause_WhenStopped_DoesNothing()
    {
        var service = new StopwatchService();
        service.Pause();
        Assert.False(service.IsRunning);
        Assert.Equal(TimeSpan.Zero, service.Elapsed);
    }

    [Fact]
    public void Pause_FreezesElapsedImmediately()
    {
        var service = new StopwatchService();
        service.Start();
        Thread.Sleep(150);
        service.Pause();
        var frozen = service.Elapsed;
        Thread.Sleep(200);
        Assert.Equal(frozen, service.Elapsed);
    }

    [Fact]
    public void Stop_ResetsElapsedToZero()
    {
        var service = new StopwatchService();
        service.Start();
        Thread.Sleep(150);
        service.Stop();
        Assert.False(service.IsRunning);
        Assert.Equal(TimeSpan.Zero, service.Elapsed);
    }

    [Fact]
    public void Tick_RaisedWhileRunning()
    {
        var service = new StopwatchService();
        var signal = new ManualResetEventSlim();
        TimeSpan? received = null;

        service.Tick += elapsed =>
        {
            received = elapsed;
            signal.Set();
        };

        service.Start();
        try
        {
            Assert.True(signal.Wait(2000), "No Tick event within 2s");
            Assert.NotNull(received);
            Assert.NotEqual(TimeSpan.Zero, received);
        }
        finally
        {
            service.Stop();
        }
    }

    [Fact]
    public void Stop_RaisesTickWithZero()
    {
        var service = new StopwatchService();
        TimeSpan? received = null;

        service.Tick += elapsed => received = elapsed;
        service.Start();
        service.Stop();

        Assert.NotNull(received);
        Assert.Equal(TimeSpan.Zero, received);
    }

    [Fact]
    public void Laps_DefaultsToEmpty()
    {
        var service = new StopwatchService();
        Assert.Empty(service.Laps);
    }

    [Fact]
    public void AddLap_AddsLapOnTop()
    {
        var service = new StopwatchService();
        service.AddLap(new StopwatchLap { Number = 1, Split = TimeSpan.FromSeconds(1), Delta = TimeSpan.FromSeconds(1) });
        service.AddLap(new StopwatchLap { Number = 2, Split = TimeSpan.FromSeconds(3), Delta = TimeSpan.FromSeconds(2) });

        Assert.Equal(2, service.Laps.Count);
        Assert.Equal(2, service.Laps[0].Number);
        Assert.Equal(1, service.Laps[1].Number);
    }

    [Fact]
    public void AddLap_IsNotMutableThroughReadOnlyList()
    {
        var service = new StopwatchService();
        service.AddLap(new StopwatchLap { Number = 1, Split = TimeSpan.Zero, Delta = TimeSpan.Zero });

        Assert.IsAssignableFrom<IReadOnlyList<StopwatchLap>>(service.Laps);
    }

    [Fact]
    public void ClearLaps_RemovesAllLaps()
    {
        var service = new StopwatchService();
        service.AddLap(new StopwatchLap { Number = 1, Split = TimeSpan.Zero, Delta = TimeSpan.Zero });

        service.ClearLaps();

        Assert.Empty(service.Laps);
    }
}