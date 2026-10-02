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
    public partial int CurrentBPM { get; set; } = AppConstants.Metronome.DEFAULT_BPM;

    [ObservableProperty]
    public partial string SelectedTimeSignature { get; set; } = AppConstants.Metronome.DEFAULT_TIME_SIGNATURE;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayMetronomeCommand))]
    public partial bool IsEnabled { get; set; } = true;

    public MetronomeViewModel(
        ILogger<MetronomeViewModel> logger,
        IMetronomeService metronomeService)
    {
        _logger = logger;
        _metronomeService = metronomeService;

        CurrentBPM = metronomeService.SavedBpm;
        SelectedTimeSignature = metronomeService.SavedTimeSignature;
    }

    public void RegisterMediaElement(object accentMediaElement, object normalMediaElement)
    {
        _metronomeService.SetMediaElement(accentMediaElement, normalMediaElement);
    }

    partial void OnCurrentBPMChanged(int value)
    {
        _metronomeService.SetTempo(value);
    }

    partial void OnSelectedTimeSignatureChanged(string value)
    {
        _metronomeService.SetTimeSignature(value);
    }

    [RelayCommand]
    private void SelectTimeSignature(string timeSignature)
    {
        _logger.LogInformation("Time signature changed to {TimeSignature}", timeSignature);
        SelectedTimeSignature = timeSignature;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task PlayMetronome()
    {
        IsEnabled = false;

        await _metronomeService.StartAsync(CurrentBPM, SelectedTimeSignature);
    }

    [RelayCommand]
    public async Task StopMetronome()
    {
        await _metronomeService.StopAsync();
        IsEnabled = true;
    }

    private bool CanStart() => IsEnabled;
}
