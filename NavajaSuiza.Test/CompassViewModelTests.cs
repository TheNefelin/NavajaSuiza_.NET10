using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class CompassViewModelTests
{
    private readonly Mock<ILogger<CompassViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ICompassService> _compassServiceMock = new();
    private readonly Mock<IOrientationService> _orientationServiceMock = new();

    private CompassViewModel CreateSut() => new(
        _loggerMock.Object,
        _languageServiceMock.Object,
        _navigationServiceMock.Object,
        _compassServiceMock.Object,
        _orientationServiceMock.Object);

    [Fact]
    public void DefaultStatusText_IsThreeDots()
    {
        var vm = CreateSut();
        Assert.Equal("...", vm.StatusText);
    }

    [Fact]
    public void DefaultAngleText_IsZero()
    {
        var vm = CreateSut();
        Assert.Equal("0°", vm.AngleText);
    }

    [Fact]
    public void DefaultCardinalDirection_IsNA()
    {
        var vm = CreateSut();
        Assert.Equal("N/A", vm.CardinalDirection);
    }

    [Fact]
    public void DefaultCompassDialRotation_IsZero()
    {
        var vm = CreateSut();
        Assert.Equal(0.0, vm.CompassDialRotation);
    }

    [Fact]
    public void StartSensors_WhenUnsupported_NavigatesBack()
    {
        _compassServiceMock.Setup(s => s.IsSupported).Returns(false);
        _orientationServiceMock.Setup(s => s.IsSupported).Returns(true);
        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns("Test");

        var vm = CreateSut();
        vm.StartSensorsCommand.Execute(null);

        _navigationServiceMock.Verify(s => s.DisplayAlertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _navigationServiceMock.Verify(s => s.GoToAsync(".."), Times.Once);
    }

    [Fact]
    public void StartSensors_WhenSupported_StartsServices()
    {
        _compassServiceMock.Setup(s => s.IsSupported).Returns(true);
        _orientationServiceMock.Setup(s => s.IsSupported).Returns(true);

        var vm = CreateSut();
        vm.StartSensorsCommand.Execute(null);

        _compassServiceMock.Verify(s => s.Start(1, true), Times.Once);
        _orientationServiceMock.Verify(s => s.Start(1), Times.Once);
    }

    [Fact]
    public void StopSensors_StopsServices()
    {
        var vm = CreateSut();
        vm.StopSensorsCommand.Execute(null);

        _compassServiceMock.Verify(s => s.Stop(), Times.Once);
        _orientationServiceMock.Verify(s => s.Stop(), Times.Once);
    }

    [Fact]
    public async Task CompassReading_UpdatesAngleAndCardinal()
    {
        _languageServiceMock.Setup(s => s.GetString("CompassCardinalNorthText")).Returns("N");
        _languageServiceMock.Setup(s => s.GetString("CompassCardinalNorthEastText")).Returns("NE");
        _compassServiceMock.Setup(s => s.IsSupported).Returns(true);
        _orientationServiceMock.Setup(s => s.IsSupported).Returns(true);

        var vm = CreateSut();
        await vm.StartSensorsCommand.ExecuteAsync(null);

        _compassServiceMock.Raise(s => s.ReadingChanged += null,
            new CompassReadingChangedEventArgs(45));

        Assert.Equal("45°", vm.AngleText);
        Assert.Equal(315, vm.CompassDialRotation);
    }
}
