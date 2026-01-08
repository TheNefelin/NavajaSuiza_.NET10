using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentNylonViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentNylonViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentNylonViewModel(
        ILogger<InstrumentNylonViewModel> logger, 
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetNylonStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentNylonViewModel] - Initializing Violin Strings in InstrumentNylonViewModel");

        IsLoading = true;
        IsBusy = false;
        //InstrumentStrings.Clear();

        // Simular carga
        await Task.Delay(500);

        // Agregar cuerdas
        //InstrumentStrings.Add(new InstrumentStringData { Note = "E2", AudioName = "GS_01_E2.wav", Description = "82.41 Hz", Thickness = 6 });
        //InstrumentStrings.Add(new InstrumentStringData { Note = "A2", AudioName = "GS_02_A2.wav", Description = "110.00 Hz", Thickness = 5 });
        //InstrumentStrings.Add(new InstrumentStringData { Note = "A4", AudioName = "GS_03_D3.wav", Description = "146.83 Hz", Thickness = 4 });
        //InstrumentStrings.Add(new InstrumentStringData { Note = "G3", AudioName = "GS_04_G3.wav", Description = "196.00 Hz", Thickness = 3 });
        //InstrumentStrings.Add(new InstrumentStringData { Note = "B3", AudioName = "GS_05_B3.wav", Description = "246.94 Hz", Thickness = 2 });
        //InstrumentStrings.Add(new InstrumentStringData { Note = "E4", AudioName = "GS_06_E4.wav", Description = "329.63 Hz", Thickness = 1 });

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentNylonViewModel] - Registering MediaElement in InstrumentNylonViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentNylonViewModel] - Clearing All String Borders in InstrumentNylonViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentNylonViewModel] - Stopping All Strings in InstrumentNylonViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}