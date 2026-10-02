namespace NavajaSuiza.Core.Interfaces;

public interface IMetronomeService
{
    int SavedBpm { get; }
    string SavedTimeSignature { get; }

    void SetMediaElement(object accentMediaElement, object normalMediaElement);
    void SetTempo(int currentBPM);
    void SetTimeSignature(string selectedTimeSignature);
    Task StartAsync(int currentBPM, string selectedTimeSignature);
    Task StopAsync();
}
