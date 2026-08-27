using System.Collections.ObjectModel;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface IInstrumentAudioService
{
    ObservableCollection<InstrumentStringData> GetNylonStringConfig();
    ObservableCollection<InstrumentStringData> GetSteelStringConfig();
    ObservableCollection<InstrumentStringData> GetBassStringConfig();
    ObservableCollection<InstrumentStringData> GetUkeleleStringConfig();
    ObservableCollection<InstrumentStringData> GetViolinStringConfig();
    ObservableCollection<InstrumentStringData> GetCharangoStringConfig();
    void RegisterMediaElement(object mediaElement);
    void RegisterStringBorder(object border, string audioName);
    Task StringTappedAsync(object border);
    Task StopAllStringAsync();
    void ClearAllBorders();
}
