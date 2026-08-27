using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class MetronomeViewModelTests
{
    private readonly Mock<IMetronomeService> _metronomeServiceMock = new();
    private readonly Mock<ILogger<MetronomeViewModel>> _loggerMock = new();

    private MetronomeViewModel CreateSut() => new(_loggerMock.Object, _metronomeServiceMock.Object);

    [Fact]
    public void DefaultBPM_Is120()
    {
        var vm = CreateSut();
        Assert.Equal(120, vm.CurrentBPM);
    }

    [Fact]
    public void DefaultTimeSignature_Is44()
    {
        var vm = CreateSut();
        Assert.Equal("4/4", vm.SelectedTimeSignature);
    }

    [Fact]
    public void IsEnabled_DefaultsToTrue()
    {
        var vm = CreateSut();
        Assert.True(vm.IsEnabled);
    }

    [Fact]
    public void SelectTimeSignature_WhenEnabled_ChangesValue()
    {
        var vm = CreateSut();
        vm.SelectTimeSignatureCommand.Execute("3/4");
        Assert.Equal("3/4", vm.SelectedTimeSignature);
    }

    [Fact]
    public void SelectTimeSignature_WhenDisabled_DoesNotChange()
    {
        var vm = CreateSut();
        vm.IsEnabled = false;
        vm.SelectTimeSignatureCommand.Execute("3/4");
        Assert.Equal("4/4", vm.SelectedTimeSignature);
    }

    [Fact]
    public void PlayMetronome_SetsIsEnabledFalse()
    {
        var vm = CreateSut();
        vm.PlayMetronomeCommand.Execute(null);
        Assert.False(vm.IsEnabled);
    }

    [Fact]
    public void PlayMetronome_CallsServiceStart()
    {
        var vm = CreateSut();
        vm.PlayMetronomeCommand.Execute(null);
        _metronomeServiceMock.Verify(s => s.Start(120, "4/4"), Times.Once);
    }

    [Fact]
    public void StopMetronome_SetsIsEnabledTrue()
    {
        var vm = CreateSut();
        vm.IsEnabled = false;
        vm.StopMetronomeCommand.Execute(null);
        Assert.True(vm.IsEnabled);
    }

    [Fact]
    public void StopMetronome_CallsServiceStop()
    {
        var vm = CreateSut();
        vm.StopMetronomeCommand.Execute(null);
        _metronomeServiceMock.Verify(s => s.Stop(), Times.Once);
    }
}
