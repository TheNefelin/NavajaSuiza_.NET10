using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MorseSignalServiceTests
{
    [Fact]
    public async Task SendAsync_EmitsFramesInOrder()
    {
        var sequence = new List<MorseFrame>
        {
            new(true, 1),
            new(false, 1),
            new(true, 1)
        };

        var emitted = new List<bool>();
        var service = new MorseSignalService();

        await service.SendAsync(sequence, isOn =>
        {
            emitted.Add(isOn);
            return Task.CompletedTask;
        }, CancellationToken.None);

        Assert.Equal(new[] { true, false, true }, emitted);
    }

    [Fact]
    public async Task SendAsync_LeavesLightAtLastFrameState()
    {
        var sequence = new List<MorseFrame> { new(true, 1), new(false, 1), new(true, 1) };
        var emitted = new List<bool>();
        var service = new MorseSignalService();

        await service.SendAsync(sequence, isOn =>
        {
            emitted.Add(isOn);
            return Task.CompletedTask;
        }, CancellationToken.None);

        Assert.True(emitted[^1]);
    }

    [Fact]
    public async Task SendAsync_Cancellation_StopsAndThrows()
    {
        var emitted = new List<bool>();
        using var cts = new CancellationTokenSource();

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var service = new MorseSignalService();
        var sendTask = service.SendAsync(
            new List<MorseFrame> { new(true, 5_000), new(true, 5_000) },
            isOn =>
            {
                emitted.Add(isOn);
                started.TrySetResult();
                return Task.CompletedTask;
            },
            cts.Token);

        await started.Task;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sendTask);
        Assert.Equal(new[] { true }, emitted);
    }
}