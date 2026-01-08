using CommunityToolkit.Maui.Views;

namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface IInstrumentAudioService
{
    void RegisterMediaElement(MediaElement mediaElement);
    void RegisterStringBorder(Border border, string audioName);
    Task StringTappedAsync(Border border);
    Task StopAllStringAsync();
    void ClearAllBorders();
}
