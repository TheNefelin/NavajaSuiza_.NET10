using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class TunerViewModelTests
{
    private readonly Mock<ILogger<TunerViewModel>> _loggerMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();

    private TunerViewModel CreateSut() => new(_loggerMock.Object, _navigationServiceMock.Object);

    [Fact]
    public void NavigateToNylon_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToNylonCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentNylonPage"), Times.Once);
    }

    [Fact]
    public void NavigateToSteel_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToSteelCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentSteelPage"), Times.Once);
    }

    [Fact]
    public void NavigateToBass_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToBassCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentBassPage"), Times.Once);
    }

    [Fact]
    public void NavigateToUkelele_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToUkeleleCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentUkulelePage"), Times.Once);
    }

    [Fact]
    public void NavigateToViolin_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToViolinCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentViolinPage"), Times.Once);
    }

    [Fact]
    public void NavigateToCharango_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToCharangoCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentCharangoPage"), Times.Once);
    }

    [Fact]
    public void NavigateCommand_WhileBusy_DoesNotNavigateTwice()
    {
        var pending = new TaskCompletionSource();
        _navigationServiceMock.Setup(s => s.PushAsync(It.IsAny<string>())).Returns(pending.Task);

        var vm = CreateSut();
        vm.NavigateToNylonCommand.Execute(null);
        vm.NavigateToSteelCommand.Execute(null);

        pending.SetResult();

        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentNylonPage"), Times.Once);
        _navigationServiceMock.Verify(s => s.PushAsync("InstrumentSteelPage"), Times.Never);
    }
}
