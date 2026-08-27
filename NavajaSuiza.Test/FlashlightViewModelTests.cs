using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class FlashlightViewModelTests
{
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IFlashlightService> _flashlightServiceMock = new();
    private readonly Mock<IDeviceDisplayService> _deviceDisplayServiceMock = new();

    private FlashlightViewModel CreateSut() => new(
        _navigationServiceMock.Object,
        _languageServiceMock.Object,
        _flashlightServiceMock.Object,
        _deviceDisplayServiceMock.Object);

    [Fact]
    public void DefaultIsFlashOn_IsFalse()
    {
        var vm = CreateSut();
        Assert.False(vm.IsFlashOn);
    }

    [Fact]
    public void DefaultIsScreenOn_IsFalse()
    {
        var vm = CreateSut();
        Assert.False(vm.IsScreenOn);
    }

    [Fact]
    public void ClickFlash_TogglesIsFlashOn()
    {
        var vm = CreateSut();
        vm.ClickFlashCommand.Execute(null);
        Assert.True(vm.IsFlashOn);
    }

    [Fact]
    public void ClickFlash_WhenOn_CallsTurnOn()
    {
        var vm = CreateSut();
        vm.ClickFlashCommand.Execute(null);
        _flashlightServiceMock.Verify(s => s.TurnOnAsync(), Times.Once);
    }

    [Fact]
    public void ClickFlash_WhenOff_CallsTurnOff()
    {
        var vm = CreateSut();
        vm.IsFlashOn = true;
        vm.ClickFlashCommand.Execute(null);
        _flashlightServiceMock.Verify(s => s.TurnOffAsync(), Times.Once);
    }

    [Fact]
    public void ClickScreen_WhenOn_NavigatesToScreenLight()
    {
        var vm = CreateSut();
        vm.ClickScreenCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("ScreenLightPage"), Times.Once);
    }

    [Fact]
    public void Cleanup_UnsubscribesLanguageChanged()
    {
        var vm = CreateSut();
        var ex = Record.Exception(() => vm.Cleanup());
        Assert.Null(ex);
    }
}
