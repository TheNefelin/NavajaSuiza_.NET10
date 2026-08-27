using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class MetronomeViewModel : BaseViewModel
{
    private readonly ILogger<MetronomeViewModel> _logger;
    private readonly IMetronomeService _metronomeService;

    [ObservableProperty]
    public partial int CurrentBPM { get; set; } = AppConstants.Metronome.DefaultBPM;

    [ObservableProperty]
    public partial string SelectedTimeSignature { get; set; } = AppConstants.Metronome.DefaultTimeSignature;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    public MetronomeViewModel(
        ILogger<MetronomeViewModel> logger,
        IMetronomeService metronomeService)
    {
        _logger = logger;
        _metronomeService = metronomeService;
    }

    public void RegisterMediaElement(object accentMediaElement, object normalMediaElement)
    {
        _metronomeService.SetMediaElement(accentMediaElement, normalMediaElement);
    }

    [RelayCommand]
    private void SelectTimeSignature(string timeSignature)
    {
        if (!IsEnabled) return;

        _logger.LogInformation("Time signature changed to {TimeSignature}", timeSignature);
        SelectedTimeSignature = timeSignature;
    }

    [RelayCommand]
    private void PlayMetronome()
    {
        IsEnabled = false;

        _metronomeService.Start(CurrentBPM, SelectedTimeSignature);
    }

    [RelayCommand]
    public void StopMetronome()
    {
        IsEnabled = true;
        _metronomeService.Stop();
    }
}
