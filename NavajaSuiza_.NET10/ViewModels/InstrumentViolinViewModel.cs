using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentViolinViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentViolinViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentViolinViewModel(
        ILogger<InstrumentViolinViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetViolinStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Initializing Violin Strings in InstrumentNylonViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Registering MediaElement in InstrumentNylonViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Clearing All String Borders in InstrumentNylonViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Stopping All Strings in InstrumentNylonViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}
