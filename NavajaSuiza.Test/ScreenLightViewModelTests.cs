using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class ScreenLightViewModelTests
{
    private readonly Mock<IDeviceDisplayService> _deviceDisplayServiceMock = new();
    private readonly Mock<IScreenBrightnessService> _brightnessServiceMock = new();

    private ScreenLightViewModel CreateSut() => new(
        _deviceDisplayServiceMock.Object,
        _brightnessServiceMock.Object);

    [Fact]
    public void InitializeScreenLight_SetsBrightnessToMax()
    {
        _brightnessServiceMock.Setup(s => s.GetCurrentBrightness()).Returns(0.5);
        var vm = CreateSut();

        vm.InitializeScreenLight();

        _brightnessServiceMock.Verify(s => s.SetScreenBrightness(1.0), Times.Once);
    }

    [Fact]
    public void InitializeScreenLight_KeepsScreenOn()
    {
        _brightnessServiceMock.Setup(s => s.GetCurrentBrightness()).Returns(0.5);
        var vm = CreateSut();

        vm.InitializeScreenLight();

        _deviceDisplayServiceMock.VerifySet(s => s.KeepScreenOn = true, Times.Once);
    }

    [Fact]
    public void Cleanup_RestoresOriginalBrightness()
    {
        _brightnessServiceMock.Setup(s => s.GetCurrentBrightness()).Returns(0.3);
        var vm = CreateSut();
        vm.InitializeScreenLight();

        vm.Cleanup();

        _brightnessServiceMock.Verify(s => s.SetScreenBrightness(0.3), Times.Once);
    }

    [Fact]
    public void Cleanup_TurnsOffScreenOn()
    {
        _brightnessServiceMock.Setup(s => s.GetCurrentBrightness()).Returns(0.5);
        var vm = CreateSut();
        vm.InitializeScreenLight();

        vm.Cleanup();

        _deviceDisplayServiceMock.VerifySet(s => s.KeepScreenOn = false, Times.Once);
    }
}
