using CommunityToolkit.Maui.Views;

namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface IMetronomeService
{
    void SetMediaElement(MediaElement accentMediaElement, MediaElement normalMediaElement);
    void Start(int currentBPM, string selectedTimeSignature);
    void Stop();
    //void Start(int currentBPM, string selectedTimeSignature);
    //void Stop();
}
