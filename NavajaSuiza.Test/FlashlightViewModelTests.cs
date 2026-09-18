using Moq;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Test;

public class FlashlightViewModelTests
{
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILanguageService> _languageServiceMock = new();
    private readonly Mock<IFlashlightService> _flashlightServiceMock = new();
    private readonly Mock<IDeviceDisplayService> _deviceDisplayServiceMock = new();
    private readonly Mock<IMorseSignalService> _morseSignalServiceMock = new();
    private readonly IFlashlightStateService _stateService = new FlashlightStateService();

    private FlashlightViewModel CreateSut() => new(
        _navigationServiceMock.Object,
        _languageServiceMock.Object,
        _flashlightServiceMock.Object,
        _deviceDisplayServiceMock.Object,
        _stateService,
        _morseSignalServiceMock.Object);

    private CancellationToken capturedToken;

    private void HangMorse(out TaskCompletionSource tcs)
    {
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _morseSignalServiceMock
            .Setup(s => s.SendAsync(It.IsAny<IReadOnlyList<MorseFrame>>(), It.IsAny<Func<bool, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyList<MorseFrame> _, Func<bool, Task> _, CancellationToken ct) =>
            {
                capturedToken = ct;
                return pending.Task;
            });
        tcs = pending;
    }

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

    [Fact]
    public async Task SendSosCommand_StartsLoop_TurnsFlashOff_AndCancelsOnSecondPress()
    {
        HangMorse(out var tcs);
        var vm = CreateSut();
        _stateService.IsFlashOn = true;

        vm.SendSosCommand.Execute(null);

        Assert.True(vm.IsSosActive);
        Assert.False(vm.IsHelpActive);
        Assert.False(vm.IsFlashOn);
        _flashlightServiceMock.Verify(s => s.TurnOffAsync(), Times.AtLeastOnce);
        _morseSignalServiceMock.Verify(
            s => s.SendAsync(
                It.Is<IReadOnlyList<MorseFrame>>(l =>
                    l.Count == MorseSignalSequence.Sos.Count + 1 && !l[l.Count - 1].IsOn),
                It.IsAny<Func<bool, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        vm.SendSosCommand.Execute(null);
        var cancelExecution = vm.SendSosCommand.ExecutionTask!;

        Assert.True(capturedToken.IsCancellationRequested);

        tcs.SetResult();
        await cancelExecution;

        Assert.False(vm.IsSosActive);
        Assert.False(vm.IsHelpActive);
    }

    [Fact]
    public async Task SendHelpCommand_WhileSosActive_CancelsSosAndStartsHelpLoop()
    {
        var tcs1 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var tokens = new List<CancellationToken>();

        _morseSignalServiceMock
            .Setup(s => s.SendAsync(It.IsAny<IReadOnlyList<MorseFrame>>(), It.IsAny<Func<bool, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyList<MorseFrame> _, Func<bool, Task> _, CancellationToken ct) =>
            {
                calls++;
                tokens.Add(ct);
                return calls == 1 ? tcs1.Task : tcs2.Task;
            });

        var vm = CreateSut();

        vm.SendSosCommand.Execute(null);
        Assert.True(vm.IsSosActive);

        vm.SendHelpCommand.Execute(null);
        var helpExecution = vm.SendHelpCommand.ExecutionTask!;

        Assert.True(tokens[0].IsCancellationRequested);

        tcs1.SetResult();
        await helpExecution;

        Assert.False(vm.IsSosActive);
        Assert.True(vm.IsHelpActive);
        _morseSignalServiceMock.Verify(
            s => s.SendAsync(It.IsAny<IReadOnlyList<MorseFrame>>(), It.IsAny<Func<bool, Task>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        vm.SendHelpCommand.Execute(null);
        var cancelExecution = vm.SendHelpCommand.ExecutionTask!;

        Assert.True(tokens[1].IsCancellationRequested);

        tcs2.SetResult();
        await cancelExecution;

        Assert.False(vm.IsHelpActive);
        Assert.False(vm.IsSosActive);
    }

    [Fact]
    public async Task ClickFlash_WhileSosActive_CancelsSosAndTurnsFlashOn()
    {
        HangMorse(out var tcs);
        var vm = CreateSut();

        vm.SendSosCommand.Execute(null);
        Assert.True(vm.IsSosActive);

        vm.ClickFlashCommand.Execute(null);
        var flashExecution = vm.ClickFlashCommand.ExecutionTask!;

        Assert.True(capturedToken.IsCancellationRequested);

        tcs.SetResult();
        await flashExecution;

        Assert.True(vm.IsFlashOn);
        Assert.False(vm.IsSosActive);
        _flashlightServiceMock.Verify(s => s.TurnOnAsync(), Times.Once);
    }

    [Fact]
    public async Task SendSosCommand_WhenFlashOn_ForceTurnsFlashOff()
    {
        HangMorse(out var tcs);
        var vm = CreateSut();
        _stateService.IsFlashOn = true;

        vm.SendSosCommand.Execute(null);

        Assert.True(vm.IsSosActive);
        Assert.False(vm.IsFlashOn);

        vm.SendSosCommand.Execute(null);
        var cancelExecution = vm.SendSosCommand.ExecutionTask!;
        tcs.SetResult();
        await cancelExecution;
    }
}