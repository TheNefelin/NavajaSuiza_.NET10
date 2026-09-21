using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class MenuViewModelTests
{
    private const string TestString = "Test";

    private readonly Mock<ILogger<MenuViewModel>> _loggerMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<IDeviceStatusService> _deviceStatusServiceMock = new();
    private readonly Mock<IFilePickerService> _filePickerServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();

    private MenuViewModel CreateSut()
    {
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns(TestString);
        return new MenuViewModel(
            _loggerMock.Object,
            _navigationServiceMock.Object,
            _deviceStatusServiceMock.Object,
            _filePickerServiceMock.Object,
            _languageServiceMock.Object);
    }

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
    public async Task NavigateToPdfReader_PicksFileAndNavigates()
    {
        const string path = "C:\\docs\\informe.pdf";
        _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>())).ReturnsAsync(path);

        var vm = CreateSut();
        await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

        _filePickerServiceMock.Verify(s => s.PickPdfAsync(TestString), Times.Once);
        _navigationServiceMock.Verify(s => s.PushAsync("PdfReaderPage", path), Times.Once);
    }

    [Fact]
    public async Task NavigateToPdfReader_WhenPickerCancelled_DoesNotNavigate()
    {
        _filePickerServiceMock.Setup(s => s.PickPdfAsync(It.IsAny<string>())).ReturnsAsync((string?)null);

        var vm = CreateSut();
        await vm.NavigateToPdfReaderCommand.ExecuteAsync(null);

        _filePickerServiceMock.Verify(s => s.PickPdfAsync(TestString), Times.Once);
        _navigationServiceMock.Verify(s => s.PushAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }
}