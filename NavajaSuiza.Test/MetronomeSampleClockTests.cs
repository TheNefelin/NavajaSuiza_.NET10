using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MetronomeSampleClockTests
{
    private const int SampleRate = 48000;
    private const int BeatsPerBufferAt120Bpm = 24000;

    [Fact]
    public void Constructor_DoesNotEmitBeatInTheFirstInterval()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);

        Assert.Empty(clock.ConsumeBeats(0, BeatsPerBufferAt120Bpm));
    }

    [Fact]
    public void ConsumeBeats_PlacesFirstBeatAfterOneIntervalAndMarksItAsAccent()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);

        var beats = clock.ConsumeBeats(0, SampleRate);

        var beat = Assert.Single(beats);
        Assert.Equal(24000, beat.OffsetInBuffer);
        Assert.True(beat.IsAccent);
    }

    [Fact]
    public void ConsumeBeats_KeepsSpacingConstantAtHighTempo()
    {
        var clock = new MetronomeSampleClock(SampleRate, 240, 4);
        var positions = new List<long>();
        long bufferStart = 0;

        while (positions.Count < 4)
        {
            foreach (var beat in clock.ConsumeBeats(bufferStart, 4800))
                positions.Add(bufferStart + beat.OffsetInBuffer);

            bufferStart += 4800;
        }

        Assert.Equal(new long[] { 12000, 24000, 36000, 48000 }, positions);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void ConsumeBeats_AccentFallsOnFirstBeatOfEveryMeasure(int beatsPerMeasure)
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, beatsPerMeasure);
        var accents = new List<int>();
        var beatCount = 0;
        long bufferStart = 0;

        for (var buffer = 0; buffer < beatsPerMeasure + 2; buffer++)
        {
            foreach (var beat in clock.ConsumeBeats(bufferStart, BeatsPerBufferAt120Bpm))
            {
                if (beat.IsAccent)
                    accents.Add(beatCount);

                beatCount++;
            }

            bufferStart += BeatsPerBufferAt120Bpm;
        }

        Assert.Equal(new[] { 0, beatsPerMeasure }, accents);
        Assert.Equal(beatsPerMeasure + 1, beatCount);
    }

    [Fact]
    public void SetTimeSignature_ResetsAccentOnTheNextBeat()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);
        clock.ConsumeBeats(0, SampleRate);

        clock.SetTimeSignature(4);
        var beats = clock.ConsumeBeats(SampleRate, SampleRate);

        Assert.Equal(2, beats.Count);
        Assert.True(beats[0].IsAccent);
        Assert.False(beats[1].IsAccent);
    }

    [Fact]
    public void SetTempo_ChangesSamplesPerBeat()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);

        clock.SetTempo(240);

        Assert.Equal(12000, clock.SamplesPerBeat);
        Assert.Equal(240, clock.Bpm);
    }

    [Fact]
    public void SetTempo_PreservesTheCurrentPhase()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);
        clock.ConsumeBeats(0, SampleRate);

        clock.SetTempo(60);

        Assert.Equal(48000, clock.NextBeatSample);
        Assert.Equal(0, Assert.Single(clock.ConsumeBeats(SampleRate, SampleRate)).OffsetInBuffer);
    }

    [Fact]
    public void ConsumeBeats_OnlyEmitsBeatsInsideTheBuffer()
    {
        var clock = new MetronomeSampleClock(SampleRate, 60, 4);

        var beats = clock.ConsumeBeats(200000, SampleRate);

        Assert.Equal(40000, Assert.Single(beats).OffsetInBuffer);
    }

    [Fact]
    public void ConsumeBeats_LargeGapTerminatesWithoutEmittingStaleBeats()
    {
        var clock = new MetronomeSampleClock(SampleRate, 60, 4);

        var beats = clock.ConsumeBeats(long.MaxValue / 2, SampleRate);

        Assert.Empty(beats);
    }

    [Fact]
    public void SetTimeSignature_IgnoresNonPositiveBeatsPerMeasure()
    {
        var clock = new MetronomeSampleClock(SampleRate, 120, 4);

        clock.SetTimeSignature(0);

        Assert.Equal(1, clock.BeatsPerMeasure);
    }
}