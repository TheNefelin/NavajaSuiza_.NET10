namespace NavajaSuiza.Core.Interfaces;

public interface IAppInfoService
{
    string Version { get; }
    string Build { get; }
}