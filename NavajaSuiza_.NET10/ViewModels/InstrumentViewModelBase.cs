using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Models;
using NavajaSuiza_.NET10.Services.Interfaces;
using NavajaSuiza.Core.ViewModels;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.ViewModels;

public abstract partial class InstrumentViewModelBase : BaseViewModel
{
    private readonly ILogger _logger;
    protected readonly IInstrumentAudioService InstrumentAudioService;

    public IInstrumentAudioService AudioService => InstrumentAudioService;

    [ObservableProperty]
    private ObservableCollection<InstrumentStringData> _instrumentStrings = new();

    protected InstrumentViewModelBase(
        ILogger logger,
        IInstrumentAudioService instrumentAudioService,
        ObservableCollection<InstrumentStringData> stringConfig)
    {
        _logger = logger;
        InstrumentAudioService = instrumentAudioService;
        InstrumentStrings = stringConfig;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing {InstrumentName} Strings", GetType().Name);

        IsLoading = true;
        IsBusy = false;

        await Task.Delay(500);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(MediaElement mediaElement)
    {
        _logger.LogInformation("Registering MediaElement in {InstrumentName}", GetType().Name);
        InstrumentAudioService.RegisterMediaElement(mediaElement);
    }

    public void ClearStringBorders()
    {
        _logger.LogInformation("Clearing All String Borders in {InstrumentName}", GetType().Name);
        InstrumentAudioService.ClearAllBorders();
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("Stopping All Strings in {InstrumentName}", GetType().Name);
        await InstrumentAudioService.StopAllStringAsync();
    }
}
