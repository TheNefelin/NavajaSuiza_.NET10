using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class InstrumentViewModelTests
{
    private readonly Mock<IInstrumentAudioService> _audioServiceMock = new();

    private static ObservableCollection<InstrumentStringData> Config() =>
        new() { new InstrumentStringData { Note = "E", AudioName = "E_01" } };

    [Fact]
    public void Constructor_LoadsBassConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetBassStringConfig()).Returns(config);
        var vm = new InstrumentBassViewModel(new Mock<ILogger<InstrumentBassViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void Constructor_LoadsCharangoConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetCharangoStringConfig()).Returns(config);
        var vm = new InstrumentCharangoViewModel(new Mock<ILogger<InstrumentCharangoViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void Constructor_LoadsNylonConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetNylonStringConfig()).Returns(config);
        var vm = new InstrumentNylonViewModel(new Mock<ILogger<InstrumentNylonViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void Constructor_LoadsSteelConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetSteelStringConfig()).Returns(config);
        var vm = new InstrumentSteelViewModel(new Mock<ILogger<InstrumentSteelViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void Constructor_LoadsUkuleleConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetUkeleleStringConfig()).Returns(config);
        var vm = new InstrumentUkuleleViewModel(new Mock<ILogger<InstrumentUkuleleViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void Constructor_LoadsViolinConfig()
    {
        var config = Config();
        _audioServiceMock.Setup(s => s.GetViolinStringConfig()).Returns(config);
        var vm = new InstrumentViolinViewModel(new Mock<ILogger<InstrumentViolinViewModel>>().Object, _audioServiceMock.Object);
        Assert.Same(config, vm.InstrumentStrings);
    }

    [Fact]
    public void RegisterMediaElement_DelegatesToAudioService()
    {
        var mediaElement = new object();
        var vm = new InstrumentViolinViewModel(new Mock<ILogger<InstrumentViolinViewModel>>().Object, _audioServiceMock.Object);

        vm.RegisterMediaElement(mediaElement);

        _audioServiceMock.Verify(s => s.RegisterMediaElement(mediaElement), Times.Once);
    }

    [Fact]
    public void ClearStringBorders_DelegatesToAudioService()
    {
        var vm = new InstrumentViolinViewModel(new Mock<ILogger<InstrumentViolinViewModel>>().Object, _audioServiceMock.Object);

        vm.ClearStringBorders();

        _audioServiceMock.Verify(s => s.ClearAllBorders(), Times.Once);
    }

    [Fact]
    public async Task StopAllStringAsync_DelegatesToAudioService()
    {
        var vm = new InstrumentViolinViewModel(new Mock<ILogger<InstrumentViolinViewModel>>().Object, _audioServiceMock.Object);

        await vm.StopAllStringAsync();

        _audioServiceMock.Verify(s => s.StopAllStringAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_FinishesWithLoadingFalseAndBusyTrue()
    {
        var vm = new InstrumentViolinViewModel(new Mock<ILogger<InstrumentViolinViewModel>>().Object, _audioServiceMock.Object);

        await vm.InitializeAsync();

        Assert.False(vm.IsLoading);
        Assert.True(vm.IsBusy);
    }
}
