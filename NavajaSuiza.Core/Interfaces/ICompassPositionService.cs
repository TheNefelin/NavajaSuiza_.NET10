using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface ICompassPositionService
{
    bool IsAvailable { get; }
    Task<PositionReading> GetCurrentPositionAsync(CancellationToken cancellationToken = default);
}