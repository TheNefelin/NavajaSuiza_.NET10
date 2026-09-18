using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class AppInfoService : IAppInfoService
{
    public string Version => AppInfo.Current.VersionString;

    public string Build => AppInfo.Current.BuildString;
}