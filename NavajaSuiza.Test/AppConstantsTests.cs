using NavajaSuiza.Core;

namespace NavajaSuiza.Test;

public class AppConstantsTests
{
    [Fact]
    public void Metronome_DefaultBPM_Is120()
    {
        Assert.Equal(120, AppConstants.Metronome.DefaultBPM);
    }

    [Fact]
    public void Metronome_DefaultTimeSignature_Is44()
    {
        Assert.Equal("4/4", AppConstants.Metronome.DefaultTimeSignature);
    }

    [Fact]
    public void Framing_MaxCanvasWidth_Is500()
    {
        Assert.Equal(500, AppConstants.Framing.MaxCanvasWidth);
    }

    [Fact]
    public void Framing_DefaultBlurIntensity_Is13()
    {
        Assert.Equal(13, AppConstants.Framing.DefaultBlurIntensity);
    }

    [Fact]
    public void Framing_AspectRatios_ContainsAllRatios()
    {
        Assert.Equal(4, AppConstants.Framing.AspectRatios.Count);
        Assert.True(AppConstants.Framing.AspectRatios.ContainsKey("1:1"));
        Assert.True(AppConstants.Framing.AspectRatios.ContainsKey("4:5"));
        Assert.True(AppConstants.Framing.AspectRatios.ContainsKey("9:16"));
        Assert.True(AppConstants.Framing.AspectRatios.ContainsKey("16:9"));
    }

    [Fact]
    public void Instruments_LoadingDelayMs_Is500()
    {
        Assert.Equal(500, AppConstants.Instruments.LoadingDelayMs);
    }
}
