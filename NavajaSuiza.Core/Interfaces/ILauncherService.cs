namespace NavajaSuiza.Core.Interfaces;

public interface ILauncherService
{
    Task<bool> OpenAsync(string uri);
}