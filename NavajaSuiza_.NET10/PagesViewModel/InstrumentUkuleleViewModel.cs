using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentUkuleleViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentUkuleleViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentUkuleleViewModel(
        ILogger<InstrumentUkuleleViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetUkeleleStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentUkuleleViewModel] - Initializing Violin Strings in InstrumentUkuleleViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentUkuleleViewModel] - Registering MediaElement in InstrumentUkuleleViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentUkuleleViewModel] - Clearing All String Borders in InstrumentUkuleleViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentUkuleleViewModel] - Stopping All Strings in InstrumentUkuleleViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}
