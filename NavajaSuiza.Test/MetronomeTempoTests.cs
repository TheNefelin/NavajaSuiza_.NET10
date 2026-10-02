using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MetronomeTempoTests
{
    [Theory]
    [InlineData(0, 50)]
    [InlineData(10, 50)]
    [InlineData(120, 120)]
    [InlineData(350, 350)]
    [InlineData(9999, 350)]
    public void Clamp_ReturnsSupportedRange(int bpm, int expected)
    {
        Assert.Equal(expected, MetronomeTempo.Clamp(bpm));
    }

    [Theory]
    [InlineData(60, 1000)]
    [InlineData(120, 500)]
    [InlineData(240, 250)]
    public void IntervalMs_MatchesTempo(int bpm, double expected)
    {
        Assert.Equal(expected, MetronomeTempo.IntervalMs(bpm), 6);
    }

    [Theory]
    [InlineData(60, 48000)]
    [InlineData(120, 24000)]
    [InlineData(240, 12000)]
    [InlineData(280, 10285.71)]
    public void SamplesPerBeat_DerivesFromAudioClock(int bpm, double expected)
    {
        Assert.Equal(expected, MetronomeTempo.SamplesPerBeat(48000, bpm), 2);
    }

    [Theory]
    [InlineData("2/2", 2)]
    [InlineData("2/4", 2)]
    [InlineData("3/4", 3)]
    [InlineData("4/4", 4)]
    [InlineData("5/4", 5)]
    [InlineData("6/8", 6)]
    [InlineData("7/8", 7)]
    [InlineData("9/9", 4)]
    public void BeatsPerMeasure_MapsTimeSignature(string timeSignature, int expected)
    {
        Assert.Equal(expected, MetronomeTempo.BeatsPerMeasure(timeSignature));
    }
}