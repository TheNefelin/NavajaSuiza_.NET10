using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MetronomeClickTests
{
    [Theory]
    [InlineData(50, 48000, 2400)]
    [InlineData(50, 44100, 2205)]
    [InlineData(50, 8000, 400)]
    public void SampleCount_MatchesDurationAndSampleRate(int durationMs, int sampleRate, int expected)
    {
        Assert.Equal(expected, MetronomeClick.SampleCount(durationMs, sampleRate));
    }

    [Fact]
    public void Render_ProducesAudibleWaveform()
    {
        var click = Render(MetronomeClick.ACCENT_FREQUENCY_HZ);

        Assert.InRange(PeakAmplitude(click), 20000, 26213);
    }

    [Fact]
    public void Render_AmplitudeDecaysTowardsTheEnd()
    {
        var click = Render(MetronomeClick.ACCENT_FREQUENCY_HZ);
        var half = click.Length / 2;

        Assert.True(PeakAmplitude(click[..half]) > PeakAmplitude(click[half..]));
    }

    [Fact]
    public void Render_AccentHasMoreZeroCrossingsThanNormal()
    {
        var accent = Render(MetronomeClick.ACCENT_FREQUENCY_HZ);
        var normal = Render(MetronomeClick.NORMAL_FREQUENCY_HZ);

        Assert.True(
            CountZeroCrossings(accent) > CountZeroCrossings(normal),
            $"acento={CountZeroCrossings(accent)} normal={CountZeroCrossings(normal)}");
    }

    [Fact]
    public void Render_WritesOnlyRequestedSampleCount()
    {
        var buffer = new short[1000];
        MetronomeClick.Render(buffer, 0, 100, 48000, MetronomeClick.ACCENT_FREQUENCY_HZ, 0.008);

        Assert.NotEqual(0, PeakAmplitude(buffer[..100]));
        Assert.Equal(0, PeakAmplitude(buffer[100..]));
    }

    [Fact]
    public void Render_HonorsDestinationOffset()
    {
        var buffer = new short[500];

        MetronomeClick.Render(buffer, 100, 50, 48000, MetronomeClick.ACCENT_FREQUENCY_HZ, 0.008);

        Assert.Equal(0, PeakAmplitude(buffer[..100]));
        Assert.NotEqual(0, PeakAmplitude(buffer[100..]));
    }

    private static short[] Render(int frequencyHz)
    {
        var click = new short[MetronomeClick.SampleCount(MetronomeClick.DURATION_MS, 48000)];

        MetronomeClick.Render(click, 0, click.Length, 48000, frequencyHz, 0.008);

        return click;
    }

    private static int PeakAmplitude(short[] samples)
    {
        var peak = 0;

        for (var i = 0; i < samples.Length; i++)
            peak = Math.Max(peak, Math.Abs(samples[i]));

        return peak;
    }

    private static int CountZeroCrossings(short[] samples)
    {
        var crossings = 0;
        var previous = samples[0] >= 0;

        for (var i = 1; i < samples.Length; i++)
        {
            var current = samples[i] >= 0;

            if (current != previous)
                crossings++;

            previous = current;
        }

        return crossings;
    }
}