using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class FlashlightService : IFlashlightService
{
    public async Task TurnOnAsync()
    {
        await Flashlight.Default.TurnOnAsync();
    }

    public async Task TurnOffAsync()
    {
        await Flashlight.Default.TurnOffAsync();
    }
}
