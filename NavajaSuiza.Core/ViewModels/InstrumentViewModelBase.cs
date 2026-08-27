using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using System.Collections.ObjectModel;

namespace NavajaSuiza.Core.ViewModels;

public abstract partial class InstrumentViewModelBase : BaseViewModel
{
    private readonly ILogger _logger;
    protected readonly IInstrumentAudioService InstrumentAudioService;

    public IInstrumentAudioService AudioService => InstrumentAudioService;

    [ObservableProperty]
    public partial ObservableCollection<InstrumentStringData> InstrumentStrings { get; set; } = new();

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

        await Task.Delay(AppConstants.Instruments.LoadingDelayMs);

        IsLoading = false;
        IsBusy = true;
    }

    public void RegisterMediaElement(object mediaElement)
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
