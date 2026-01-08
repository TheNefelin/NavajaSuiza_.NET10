using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentBassViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentBassViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentBassViewModel(
        ILogger<InstrumentBassViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetBassStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentBassViewModel] - Initializing Violin Strings in InstrumentBassViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentBassViewModel] - Registering MediaElement in InstrumentBassViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentBassViewModel] - Clearing All String Borders in InstrumentBassViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentBassViewModel] - Stopping All Strings in InstrumentBassViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}
