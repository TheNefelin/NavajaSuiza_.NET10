using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class MetronomeViewModelTests
{
    private readonly Mock<IMetronomeService> _metronomeServiceMock = new();
    private readonly Mock<ILogger<MetronomeViewModel>> _loggerMock = new();

    private MetronomeViewModel CreateSut(int savedBpm = 120, string savedTimeSignature = "4/4")
    {
        _metronomeServiceMock.SetupGet(s => s.SavedBpm).Returns(savedBpm);
        _metronomeServiceMock.SetupGet(s => s.SavedTimeSignature).Returns(savedTimeSignature);
        _metronomeServiceMock.Setup(s => s.StopAsync()).Returns(Task.CompletedTask);
        _metronomeServiceMock.Setup(s => s.StartAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        return new(_loggerMock.Object, _metronomeServiceMock.Object);
    }

    [Fact]
    public void Constructor_LoadsSavedBpm()
    {
        var vm = CreateSut(savedBpm: 90);
        Assert.Equal(90, vm.CurrentBPM);
    }

    [Fact]
    public void Constructor_LoadsSavedTimeSignature()
    {
        var vm = CreateSut(savedTimeSignature: "7/8");
        Assert.Equal("7/8", vm.SelectedTimeSignature);
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
    public void SelectTimeSignature_WhilePlaying_ChangesValueAndNotifiesService()
    {
        var vm = CreateSut();
        vm.IsEnabled = false;
        vm.SelectTimeSignatureCommand.Execute("7/8");
        Assert.Equal("7/8", vm.SelectedTimeSignature);
        _metronomeServiceMock.Verify(s => s.SetTimeSignature("7/8"), Times.Once);
    }

    [Fact]
    public void CurrentBPM_Change_NotifiesServiceSetTempo()
    {
        var vm = CreateSut();
        vm.CurrentBPM = 200;
        _metronomeServiceMock.Verify(s => s.SetTempo(200), Times.Once);
    }

    [Fact]
    public void PlayMetronome_DoesNotNotifyServiceSetTempo()
    {
        var vm = CreateSut();
        _metronomeServiceMock.Invocations.Clear();
        vm.PlayMetronomeCommand.Execute(null);
        _metronomeServiceMock.Verify(s => s.SetTempo(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Constructor_NotifiesServiceWithSavedValues()
    {
        CreateSut(savedBpm: 90, savedTimeSignature: "6/8");
        _metronomeServiceMock.Verify(s => s.SetTempo(90), Times.Once);
        _metronomeServiceMock.Verify(s => s.SetTimeSignature("6/8"), Times.Once);
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
        _metronomeServiceMock.Verify(s => s.StartAsync(120, "4/4"), Times.Once);
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
        _metronomeServiceMock.Verify(s => s.StopAsync(), Times.Once);
    }
}
