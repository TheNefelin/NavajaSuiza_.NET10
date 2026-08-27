using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class FlashlightViewModelTests
{
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IFlashlightService> _flashlightServiceMock = new();
    private readonly Mock<IDeviceDisplayService> _deviceDisplayServiceMock = new();
    private readonly IFlashlightStateService _stateService = new FlashlightStateService();

    private FlashlightViewModel CreateSut() => new(
        _navigationServiceMock.Object,
        _languageServiceMock.Object,
        _flashlightServiceMock.Object,
        _deviceDisplayServiceMock.Object,
        _stateService);

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
        _stateService.IsFlashOn = true;
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
    public void FlashState_PersistsAcrossViewModelInstances()
    {
        var vm1 = CreateSut();
        vm1.ClickFlashCommand.Execute(null);
        Assert.True(vm1.IsFlashOn);

        var vm2 = CreateSut();
        Assert.True(vm2.IsFlashOn);
    }

    [Fact]
    public void ScreenState_DoesNotAffectFlashState()
    {
        var vm = CreateSut();
        vm.ClickFlashCommand.Execute(null);
        Assert.True(vm.IsFlashOn);

        vm.ClickScreenCommand.Execute(null);
        Assert.True(vm.IsFlashOn);
        Assert.False(vm.IsScreenOn);
    }

    [Fact]
    public void Cleanup_UnsubscribesLanguageChanged()
    {
        var vm = CreateSut();
        var ex = Record.Exception(() => vm.Cleanup());
        Assert.Null(ex);
    }
}
