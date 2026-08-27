using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Test;

public class InstrumentStringDataTests
{
    [Fact]
    public void Defaults_AreEmptyStrings()
    {
        var data = new InstrumentStringData();
        Assert.Equal(string.Empty, data.Note);
        Assert.Equal(string.Empty, data.AudioName);
        Assert.Equal(string.Empty, data.Description);
        Assert.Equal(0, data.Thickness);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var data = new InstrumentStringData
        {
            Note = "E2",
            AudioName = "GN_01_E2.wav",
            Description = "82.41 Hz",
            Thickness = 6
        };

        Assert.Equal("E2", data.Note);
        Assert.Equal("GN_01_E2.wav", data.AudioName);
        Assert.Equal("82.41 Hz", data.Description);
        Assert.Equal(6, data.Thickness);
    }
}
