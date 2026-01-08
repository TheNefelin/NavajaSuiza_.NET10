using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentCharangoViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentCharangoViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentCharangoViewModel(
        ILogger<InstrumentCharangoViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetCharangoStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentCharangoViewModel] - Initializing Violin Strings in InstrumentCharangoViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentCharangoViewModel] - Registering MediaElement in InstrumentCharangoViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentCharangoViewModel] - Clearing All String Borders in InstrumentCharangoViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentCharangoViewModel] - Stopping All Strings in InstrumentCharangoViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}
