using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MetronomeEngineTests
{
    [Fact]
    public async Task StartAsync_StopsThePreviousSessionBeforeStartingANewOne()
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        await engine.StartAsync(120, 4);
        await engine.StartAsync(140, 3);

        Assert.Equal(["Stop", "Start:120:4", "Stop", "Start:140:3"], player.Calls);
        Assert.True(engine.IsRunning);
    }

    [Fact]
    public async Task StartAsync_ConcurrentStarts_LeaveOnlyOneActiveSession()
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(i => engine.StartAsync(100 + i, 4)));

        Assert.True(engine.IsRunning);
        AssertEveryStartIsPrecededByStop(player.Calls);
    }

    [Fact]
    public async Task StopAsync_ConcurrentWithStart_LeavesNoSessionRunning()
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        for (var i = 0; i < 25; i++)
            await Task.WhenAll(engine.StartAsync(120, 4), engine.StopAsync());

        await engine.StopAsync();

        Assert.False(engine.IsRunning);
        Assert.Equal("Stop", player.Calls[^1]);
        AssertEveryStartIsPrecededByStop(player.Calls);
    }

    /// <summary>
    /// La garantía de sesión única: nunca hay dos arranques vivos a la vez porque cada <c>Start</c>
    /// va inmediatamente precedido de un <c>Stop</c>, sin importar el orden en que se intercalen las
    /// llamadas concurrentes.
    /// </summary>
    private static void AssertEveryStartIsPrecededByStop(List<string> calls)
    {
        for (var i = 1; i < calls.Count; i++)
        {
            if (calls[i].StartsWith("Start"))
                Assert.Equal("Stop", calls[i - 1]);
        }
    }

    [Fact]
    public async Task StopAsync_IsIdempotent()
    {
        var engine = new MetronomeEngine(new RecordingPlayer());

        await engine.StopAsync();
        await engine.StopAsync();

        Assert.False(engine.IsRunning);
    }

    [Fact]
    public async Task StartAsync_ClampsBpmToTheSupportedRange()
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        await engine.StartAsync(10, 4);

        Assert.Equal(50, engine.Bpm);
        Assert.Equal("Start:50:4", player.Calls[^1]);
    }

    [Theory]
    [InlineData(10, 50)]
    [InlineData(120, 120)]
    [InlineData(9999, 350)]
    public void SetTempo_ClampsAndForwardsToThePlayer(int bpm, int expected)
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        engine.SetTempo(bpm);

        Assert.Equal(expected, engine.Bpm);
        Assert.Equal(expected, player.Tempo);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(6, 6)]
    [InlineData(7, 7)]
    public void SetTimeSignature_ForwardsBeatsPerMeasureToThePlayer(int beatsPerMeasure, int expected)
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        engine.SetTimeSignature(beatsPerMeasure);

        Assert.Equal(expected, engine.BeatsPerMeasure);
        Assert.Equal(expected, player.BeatsPerMeasure);
    }

    [Fact]
    public async Task StartAsync_AfterStop_AppliesTheLatestTempoAndSignature()
    {
        var player = new RecordingPlayer();
        var engine = new MetronomeEngine(player);

        await engine.StartAsync(120, 4);
        await engine.StopAsync();

        engine.SetTempo(200);
        engine.SetTimeSignature(7);

        await engine.StartAsync(200, 7);

        Assert.Equal("Start:200:7", player.Calls[^1]);
    }

    private sealed class RecordingPlayer : IMetronomePlayer
    {
        public List<string> Calls { get; } = [];

        public int Tempo { get; private set; }

        public int BeatsPerMeasure { get; private set; }

        public void Start(int bpm, int beatsPerMeasure) => Calls.Add($"Start:{bpm}:{beatsPerMeasure}");

        public void SetTempo(int bpm) => Tempo = bpm;

        public void SetTimeSignature(int beatsPerMeasure) => BeatsPerMeasure = beatsPerMeasure;

        public void Stop() => Calls.Add("Stop");

        public void AttachMediaSinks(object? accentMediaElement, object? normalMediaElement)
        {
        }

        public void Dispose()
        {
        }
    }
}