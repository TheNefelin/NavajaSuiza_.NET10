using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentSteelViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentSteelViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    public InstrumentSteelViewModel(
        ILogger<InstrumentSteelViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;

        // Agregar cuerdas
        InstrumentStrings = _instrumentAudioService.GetSteelStringConfig();
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("[InstrumentSteelViewModel] - Initializing Violin Strings in InstrumentSteelViewModel");

        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentSteelViewModel] - Registering MediaElement in InstrumentSteelViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentSteelViewModel] - Clearing All String Borders in InstrumentSteelViewModel");
        _instrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentSteelViewModel] - Stopping All Strings in InstrumentSteelViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }
}
