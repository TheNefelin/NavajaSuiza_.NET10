using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Services;

public class MorseSignalService : IMorseSignalService
{
    public async Task SendAsync(IReadOnlyList<MorseFrame> sequence, Func<bool, Task> emit, CancellationToken cancellationToken)
    {
        foreach (var frame in sequence)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await emit(frame.IsOn).ConfigureAwait(false);
            await Task.Delay(frame.DurationMs, cancellationToken).ConfigureAwait(false);
        }
    }
}