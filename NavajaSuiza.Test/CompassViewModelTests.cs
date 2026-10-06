using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class CompassViewModelTests
{
    private readonly Mock<ILogger<CompassViewModel>> _loggerMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ICompassService> _compassServiceMock = new();
    private readonly Mock<IOrientationService> _orientationServiceMock = new();
    private readonly Mock<ICompassPositionService> _compassPositionServiceMock = new();
    private readonly Mock<ITimeSource> _timeSourceMock = new();

    private CompassViewModel CreateSut() => new(
        _loggerMock.Object,
        _languageServiceMock.Object,
        _navigationServiceMock.Object,
        _compassServiceMock.Object,
        _orientationServiceMock.Object,
        _compassPositionServiceMock.Object,
        _timeSourceMock.Object);

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

        _compassServiceMock.Verify(s => s.Start(true), Times.Once);
        _orientationServiceMock.Verify(s => s.Start(), Times.Once);
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

    [Fact]
    public void Locate_DefaultsToNoPositionShown()
    {
        var vm = CreateSut();

        Assert.False(vm.HasPosition);
        Assert.Equal("--", vm.LatitudeText);
        Assert.Equal("--", vm.LongitudeText);
        Assert.Equal("--", vm.AltitudeText);
        Assert.Equal("--", vm.AccuracyText);
    }

    [Fact]
    public async Task Locate_WhenAvailable_FillsCoordinates()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionReading(-47.39056, -123.04567, 1200, 8));

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.True(vm.HasPosition);
        Assert.Equal("S 47° 23.434'", vm.LatitudeText);
        Assert.Equal("W 123° 2.740'", vm.LongitudeText);
        Assert.Equal("1200 m", vm.AltitudeText);
        Assert.Equal("≈8 m", vm.AccuracyText);
    }

    [Fact]
    public async Task Locate_WithoutAltitude_ShowsUnavailable()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionReading(10.5, 20.25, null, 15));

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.True(vm.HasPosition);
        Assert.Equal("N 10° 30.000'", vm.LatitudeText);
        Assert.Equal("E 20° 15.000'", vm.LongitudeText);
        Assert.Equal("no disponible", vm.AltitudeText);
    }

    [Fact]
    public async Task Locate_WithZeroAltitude_ShowsUnavailable()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionReading(10.5, 20.25, 0, 15));

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.Equal("no disponible", vm.AltitudeText);
    }

    [Fact]
    public async Task Locate_WhenLocationDisabled_ShowsMessageAndNoPosition()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(false);

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.False(vm.HasPosition);
        Assert.Equal("Debes activar la Ubicación en el dispositivo.", vm.PositionMessage);
        Assert.True(vm.IsLocationDisabledMessage);
        _compassPositionServiceMock.Verify(
            s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Locate_WhenPermissionDenied_DoesNotFlagLocationDisabled()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.False(vm.IsLocationDisabledMessage);
    }

    [Fact]
    public async Task Locate_WhenSuccess_ClearsLocationDisabledFlag()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PositionReading(10, 20, 100, 15));

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.False(vm.IsLocationDisabledMessage);
        Assert.True(vm.HasPosition);
    }

    [Fact]
    public async Task Locate_WhenPermissionDenied_ShowsPermissionMessage()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.False(vm.HasPosition);
        Assert.Equal(
            "Se necesita permiso de ubicación para mostrar la posición.",
            vm.PositionMessage);
    }

    [Fact]
    public async Task Locate_WhenReadingFails_ShowsGenericMessageAndClearsLocating()
    {
        _compassPositionServiceMock.Setup(s => s.IsAvailable).Returns(true);
        _compassPositionServiceMock
            .Setup(s => s.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException());

        var vm = CreateSut();
        await vm.LocateCommand.ExecuteAsync(null);

        Assert.False(vm.HasPosition);
        Assert.False(vm.IsLocating);
        Assert.Equal(
            "No se pudo obtener la posición. Salí al exterior e intenta de nuevo.",
            vm.PositionMessage);
    }
}
