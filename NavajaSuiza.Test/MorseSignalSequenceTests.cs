using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Test;

public class MorseSignalSequenceTests
{
    [Fact]
    public void Sos_HasExpectedFrameCount()
    {
        var frames = MorseSignalSequence.Sos;
        Assert.Equal(17, frames.Count);
    }

    [Fact]
    public void Sos_FirstFrame_IsOnForOneUnit()
    {
        var first = MorseSignalSequence.Sos[0];
        Assert.True(first.IsOn);
        Assert.Equal(MorseSignalSequence.DOT_MS, first.DurationMs);
    }

    [Fact]
    public void Sos_IntraCharacterGap_IsOneUnit()
    {
        var gaps = MorseSignalSequence.Sos.Where(f => !f.IsOn && f.DurationMs == MorseSignalSequence.INTRACHAR_GAP_MS).ToList();
        Assert.Equal(6, gaps.Count);
    }

    [Fact]
    public void Sos_InterLetterGap_IsThreeUnits()
    {
        var characterGaps = MorseSignalSequence.Sos
            .Where(f => !f.IsOn && f.DurationMs == MorseSignalSequence.CHAR_GAP_MS)
            .ToList();

        Assert.Equal(2, characterGaps.Count);
    }

    [Fact]
    public void Sos_ContainsDashes()
    {
        var dash = MorseSignalSequence.Sos.FirstOrDefault(f => f.IsOn && f.DurationMs == MorseSignalSequence.DASH_MS);
        Assert.NotNull(dash);
    }

    [Fact]
    public void Sos_LastFrame_IsOnForOneUnit()
    {
        var last = MorseSignalSequence.Sos[^1];
        Assert.True(last.IsOn);
        Assert.Equal(MorseSignalSequence.DOT_MS, last.DurationMs);
    }

    [Fact]
    public void Help_HasExpectedFrameCount()
    {
        Assert.Equal(25, MorseSignalSequence.Help.Count);
    }

    [Fact]
    public void Help_ContainsIntraCharacterGapsAndDashes()
    {
        var frames = MorseSignalSequence.Help;
        var intraGaps = frames.Where(f => !f.IsOn && f.DurationMs == MorseSignalSequence.INTRACHAR_GAP_MS).ToList();

        Assert.Equal(9, intraGaps.Count);
        Assert.Contains(frames, f => f.IsOn && f.DurationMs == MorseSignalSequence.DASH_MS);
    }

    [Fact]
    public void Build_HandlesWordSpaces()
    {
        var frames = MorseSignalSequence.Build("SOS SOS");
        Assert.Contains(frames, f => !f.IsOn && f.DurationMs == MorseSignalSequence.WORD_GAP_MS);
    }

    [Fact]
    public void Build_ThrowsForUnsupportedCharacter()
    {
        Assert.Throws<ArgumentException>(() => MorseSignalSequence.Build("SOS?"));
    }

    [Fact]
    public void Build_ThrowsForEmptyWord()
    {
        Assert.Throws<ArgumentException>(() => MorseSignalSequence.Build(" "));
    }
}