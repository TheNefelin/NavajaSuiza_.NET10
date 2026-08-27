namespace NavajaSuiza.Core.Interfaces;

public interface IMetronomeService
{
    void SetMediaElement(object accentMediaElement, object normalMediaElement);
    void Start(int currentBPM, string selectedTimeSignature);
    void Stop();
}
