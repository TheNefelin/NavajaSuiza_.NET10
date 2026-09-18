using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class LauncherService : ILauncherService
{
    public async Task<bool> OpenAsync(string uri)
    {
        return await Launcher.Default.OpenAsync(uri);
    }
}