using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.ViewModels;

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
        _logger.LogInformation("[InstrumentNylonViewModel] - Initializing Nylon Strings in InstrumentNylonViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

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