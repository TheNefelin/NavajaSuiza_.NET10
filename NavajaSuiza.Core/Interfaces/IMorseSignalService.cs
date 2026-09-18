using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface IMorseSignalService
{
    Task SendAsync(IReadOnlyList<MorseFrame> sequence, Func<bool, Task> emit, CancellationToken cancellationToken);
}