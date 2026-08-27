using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class MenuViewModelTests
{
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<IDeviceStatusService> _deviceStatusServiceMock = new();

    private MenuViewModel CreateSut() => new(_navigationServiceMock.Object, _deviceStatusServiceMock.Object);

    [Fact]
    public void BatteryLevel_DefaultsToZero()
    {
        var vm = CreateSut();
        Assert.Equal("0%", vm.BatteryLevel);
    }

    [Fact]
    public void AvailableStorage_DefaultsToZeroGB()
    {
        var vm = CreateSut();
        Assert.Equal("0 GB", vm.AvailableStorage);
    }

    [Fact]
    public void OnPageAppearing_UpdatesBatteryAndStorage()
    {
        _deviceStatusServiceMock.Setup(s => s.GetBatteryLevel()).Returns("85%");
        _deviceStatusServiceMock.Setup(s => s.GetAvailableStorage()).Returns("32 GB");

        var vm = CreateSut();
        vm.OnPageAppearing();

        Assert.Equal("85%", vm.BatteryLevel);
        Assert.Equal("32 GB", vm.AvailableStorage);
    }

    [Fact]
    public void NavigateToFlashlight_CallsNavigationService()
    {
        var vm = CreateSut();
        vm.NavigateToFlashlightCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.PushAsync("FlashlightPage"), Times.Once);
    }

    [Fact]
    public void NavigateToAbout_CallsGoToAsync()
    {
        var vm = CreateSut();
        vm.NavigateToAboutCommand.Execute(null);
        _navigationServiceMock.Verify(s => s.GoToAsync("//AboutPage"), Times.Once);
    }
}
