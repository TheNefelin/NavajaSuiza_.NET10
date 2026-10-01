using Microsoft.Extensions.Logging;
using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class StopwatchViewModelTests
{
    private readonly Mock<ILogger<StopwatchViewModel>> _loggerMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();

    private (StopwatchViewModel Vm, Mock<IStopwatchService> Service, Action<TimeSpan> SetElapsed) CreateSut(
        bool running = false,
        bool confirmDelete = true)
    {
        var elapsed = TimeSpan.Zero;
        var laps = new List<StopwatchLap>();
        var service = new Mock<IStopwatchService>();
        service.Setup(s => s.IsRunning).Returns(() => running);
        service.Setup(s => s.Elapsed).Returns(() => elapsed);
        service.Setup(s => s.Start()).Callback(() => running = true);
        service.Setup(s => s.Pause()).Callback(() => running = false);
        service.Setup(s => s.Stop()).Callback(() =>
        {
            running = false;
            elapsed = TimeSpan.Zero;
        });
        service.Setup(s => s.Laps).Returns(() => laps.ToArray());
        service.Setup(s => s.AddLap(It.IsAny<StopwatchLap>())).Callback<StopwatchLap>(lap => laps.Insert(0, lap));
        service.Setup(s => s.ClearLaps()).Callback(() => laps.Clear());
        service.Setup(s => s.RemoveLap(It.IsAny<int>())).Callback<int>(index =>
        {
            if (index < 0 || index >= laps.Count)
                return;

            laps.RemoveAt(index);

            for (var i = 0; i < laps.Count; i++)
            {
                var previousSplit = i + 1 < laps.Count ? laps[i + 1].Split : TimeSpan.Zero;
                var number = laps.Count - i;

                if (laps[i].Number != number)
                {
                    laps[i] = new StopwatchLap
                    {
                        Number = number,
                        Split = laps[i].Split,
                        Delta = laps[i].Split - previousSplit
                    };
                }
            }
        });

        _languageServiceMock.Setup(s => s.GetString(It.IsAny<string>())).Returns("test");
        _navigationServiceMock
            .Setup(n => n.DisplayAlertConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(confirmDelete);

        var vm = new StopwatchViewModel(
            _loggerMock.Object,
            service.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object);
        return (vm, service, value => elapsed = value);
    }

    [Fact]
    public void ElapsedText_DefaultsToZero()
    {
        var (vm, _, _) = CreateSut();
        Assert.Equal("00:00:00.000", vm.ElapsedText);
    }

    [Fact]
    public void IsRunning_DefaultsToFalse()
    {
        var (vm, _, _) = CreateSut();
        Assert.False(vm.IsRunning);
    }

    [Theory]
    [InlineData("00:00:00.000", 0, 0, 0, 0)]
    [InlineData("00:01:02.003", 0, 1, 2, 3)]
    [InlineData("01:00:00.000", 1, 0, 0, 0)]
    [InlineData("00:00:00.100", 0, 0, 0, 100)]
    [InlineData("00:00:00.350", 0, 0, 0, 350)]
    public void FormatTimeSpan_ReturnsFormattedText(string expected, int hours, int minutes, int seconds, int millis)
    {
        var elapsed = new TimeSpan(0, hours, minutes, seconds, millis);
        Assert.Equal(expected, StopwatchViewModel.FormatTimeSpan(elapsed));
    }

    [Fact]
    public void Initialize_SyncsStateFromService()
    {
        var (vm, _, _) = CreateSut(running: true);
        vm.Initialize();
        Assert.True(vm.IsRunning);
        Assert.Equal("00:00:00.000", vm.ElapsedText);
    }

    [Fact]
    public void Tick_UpdatesElapsedText()
    {
        var (vm, service, _) = CreateSut();
        vm.Initialize();
        service.Raise(s => s.Tick += null, TimeSpan.FromMinutes(1));
        Assert.Equal("00:01:00.000", vm.ElapsedText);
    }

    [Fact]
    public void Cleanup_UnsubscribesFromTick()
    {
        var (vm, service, _) = CreateSut();
        vm.Initialize();
        vm.Cleanup();
        service.Raise(s => s.Tick += null, TimeSpan.FromMinutes(1));
        Assert.Equal("00:00:00.000", vm.ElapsedText);
    }

    [Fact]
    public void StartLap_WhenNotRunning_CallsStart()
    {
        var (vm, service, _) = CreateSut();
        vm.Initialize();
        vm.StartLapCommand.Execute(null);
        service.Verify(s => s.Start(), Times.Once);
        Assert.True(vm.IsRunning);
        Assert.Empty(vm.Laps);
    }

    [Fact]
    public void StartLap_WhenRunning_AddsLap()
    {
        var (vm, service, setElapsed) = CreateSut(running: true);
        vm.Initialize();
        setElapsed(TimeSpan.FromSeconds(10));

        vm.StartLapCommand.Execute(null);

        service.Verify(s => s.Start(), Times.Never);
        var lap = Assert.Single(vm.Laps);
        Assert.Equal(1, lap.Number);
        Assert.Equal(TimeSpan.FromSeconds(10), lap.Split);
        Assert.Equal(TimeSpan.FromSeconds(10), lap.Delta);
        Assert.Equal("00:00:10.000", lap.SplitText);
        Assert.Equal("+00:00:10.000", lap.DeltaText);
    }

    [Fact]
    public void StartLap_WhenRunningTwice_AddsTwoLapsWithDelta()
    {
        var (vm, _, setElapsed) = CreateSut(running: true);
        vm.Initialize();

        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);

        setElapsed(TimeSpan.FromSeconds(15));
        vm.StartLapCommand.Execute(null);

        Assert.Equal(2, vm.Laps.Count);
        Assert.Equal(2, vm.Laps[0].Number);
        Assert.Equal(TimeSpan.FromSeconds(15), vm.Laps[0].Split);
        Assert.Equal(TimeSpan.FromSeconds(5), vm.Laps[0].Delta);
        Assert.Equal(1, vm.Laps[1].Number);
    }

    [Fact]
    public void StopReset_WhenRunning_CallsPauseAndKeepsLaps()
    {
        var (vm, service, setElapsed) = CreateSut(running: true);
        vm.Initialize();
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);

        vm.StopResetCommand.Execute(null);

        service.Verify(s => s.Pause(), Times.Once);
        service.Verify(s => s.Stop(), Times.Never);
        Assert.False(vm.IsRunning);
        Assert.Single(vm.Laps);
    }

    [Fact]
    public void StopReset_WhenStopped_ResetsAndClearsLaps()
    {
        var (vm, service, setElapsed) = CreateSut(running: true);
        vm.Initialize();
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);
        vm.StopResetCommand.Execute(null);

        vm.StopResetCommand.Execute(null);

        service.Verify(s => s.Stop(), Times.Once);
        Assert.False(vm.IsRunning);
        Assert.Empty(vm.Laps);
        Assert.Equal("00:00:00.000", vm.ElapsedText);
    }

    [Fact]
    public void Initialize_RestoresLapsFromService_AfterNewViewModel()
    {
        var (_, service, setElapsed) = CreateSut(running: true);
        var vm = new StopwatchViewModel(
            _loggerMock.Object,
            service.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object);
        vm.Initialize();
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);
        vm.Cleanup();

        var vm2 = new StopwatchViewModel(
            _loggerMock.Object,
            service.Object,
            _navigationServiceMock.Object,
            _languageServiceMock.Object);
        vm2.Initialize();

        var lap = Assert.Single(vm2.Laps);
        Assert.Equal(1, lap.Number);
        Assert.Equal(TimeSpan.FromSeconds(10), lap.Split);
    }

    [Fact]
    public async Task DeleteLapAsync_WhenConfirmed_RemovesLap()
    {
        var (vm, _, setElapsed) = CreateSut(running: true);
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);
        setElapsed(TimeSpan.FromSeconds(25));
        vm.StartLapCommand.Execute(null);

        var target = vm.Laps.First(l => l.Number == 2);
        await vm.DeleteLapCommand.ExecuteAsync(target);

        var lap = Assert.Single(vm.Laps);
        Assert.Equal(1, lap.Number);
        Assert.Equal(TimeSpan.FromSeconds(10), lap.Split);
    }

    [Fact]
    public async Task DeleteLapAsync_WhenNotConfirmed_KeepsLaps()
    {
        var (vm, _, setElapsed) = CreateSut(running: true, confirmDelete: false);
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);
        setElapsed(TimeSpan.FromSeconds(25));
        vm.StartLapCommand.Execute(null);

        await vm.DeleteLapCommand.ExecuteAsync(vm.Laps[0]);

        Assert.Equal(2, vm.Laps.Count);
    }

    [Fact]
    public async Task DeleteLapAsync_MiddleLap_KeepsCorrelativesDense()
    {
        var (vm, _, setElapsed) = CreateSut(running: true);
        setElapsed(TimeSpan.FromSeconds(10));
        vm.StartLapCommand.Execute(null);
        setElapsed(TimeSpan.FromSeconds(25));
        vm.StartLapCommand.Execute(null);
        setElapsed(TimeSpan.FromSeconds(40));
        vm.StartLapCommand.Execute(null);

        Assert.Equal(3, vm.Laps[0].Number);

        await vm.DeleteLapCommand.ExecuteAsync(vm.Laps[1]);

        Assert.Equal(2, vm.Laps.Count);
        Assert.Equal(2, vm.Laps[0].Number);
        Assert.Equal(TimeSpan.FromSeconds(40), vm.Laps[0].Split);
        Assert.Equal(TimeSpan.FromSeconds(30), vm.Laps[0].Delta);
        Assert.Equal(1, vm.Laps[1].Number);
        Assert.Equal(TimeSpan.FromSeconds(10), vm.Laps[1].Split);
    }
}