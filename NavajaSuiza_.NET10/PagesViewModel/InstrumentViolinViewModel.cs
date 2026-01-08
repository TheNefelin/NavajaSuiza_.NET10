using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class InstrumentViolinViewModel : BaseViewModel
{
    private readonly ILogger<InstrumentViolinViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _violinStrings = new();

    public InstrumentViolinViewModel(
        ILogger<InstrumentViolinViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        IsBusy = false;

        // Simular carga
        await Task.Delay(500);

        // Agregar cuerdas
        _violinStrings.Clear();

        ViolinStrings.Add(new InstrumentStringData { Note = "G3", AudioName = "V_01_G2.wav", Description = "196.00 Hz", Thickness = 1 });
        ViolinStrings.Add(new InstrumentStringData { Note = "D4", AudioName = "V_02_D3.wav", Description = "293.66 Hz", Thickness = 1 });
        ViolinStrings.Add(new InstrumentStringData { Note = "A4", AudioName = "V_03_A3.wav", Description = "440.00 Hz", Thickness = 1 });
        ViolinStrings.Add(new InstrumentStringData { Note = "E5", AudioName = "V_04_E4.wav", Description = "659.26 Hz", Thickness = 1 });

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Registering MediaElement in TestingViewModel");
        _instrumentAudioService.RegisterMediaElement(mediaElement);
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Stopping All Strings in TestingViewModel");
        await _instrumentAudioService.StopAllStringAsync();
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("[InstrumentViolinViewModel] - Clearing All String Borders in TestingViewModel");
        _instrumentAudioService.ClearAllBorders();
    }
}
