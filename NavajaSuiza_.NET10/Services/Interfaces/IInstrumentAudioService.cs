using CommunityToolkit.Maui.Views;
using NavajaSuiza_.NET10.Models;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface IInstrumentAudioService
{
    ObservableCollection<InstrumentStringData> GetNylonStringConfig();
    ObservableCollection<InstrumentStringData> GetSteelStringConfig();
    ObservableCollection<InstrumentStringData> GetBassStringConfig();
    ObservableCollection<InstrumentStringData> GetUkeleleStringConfig();
    ObservableCollection<InstrumentStringData> GetViolinStringConfig();
    ObservableCollection<InstrumentStringData> GetCharangoStringConfig();
    void RegisterMediaElement(MediaElement mediaElement);
    void RegisterStringBorder(Border border, string audioName);
    Task StringTappedAsync(Border border);
    Task StopAllStringAsync();
    void ClearAllBorders();
}
