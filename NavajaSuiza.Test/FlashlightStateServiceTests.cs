using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class FlashlightStateServiceTests
{
    [Fact]
    public void IsFlashOn_DefaultsToFalse()
    {
        var service = new FlashlightStateService();
        Assert.False(service.IsFlashOn);
    }

    [Fact]
    public void IsFlashOn_SetToTrue_ReturnsTrue()
    {
        var service = new FlashlightStateService();
        service.IsFlashOn = true;
        Assert.True(service.IsFlashOn);
    }

    [Fact]
    public void IsFlashOn_ConcurrentWrites_DoesNotThrow()
    {
        var service = new FlashlightStateService();

        Parallel.For(0, 1000, i => service.IsFlashOn = i % 2 == 0);

        _ = service.IsFlashOn;
    }
}
