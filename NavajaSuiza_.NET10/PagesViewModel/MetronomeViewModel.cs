using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class MetronomeViewModel : BaseViewModel
{
    private readonly ILogger<MetronomeViewModel> _logger;
    private readonly IMetronomeService _metronomeService;

    [ObservableProperty]
    private int _currentBPM = 120;

    [ObservableProperty]
    private string _selectedTimeSignature = "4/4";

    [ObservableProperty]
    private bool _isEnabled = true;

    public MetronomeViewModel(
        ILogger<MetronomeViewModel> logger,
        IMetronomeService metronomeService)
    {
        _logger = logger;
        _metronomeService = metronomeService;
    }

    public void RegisterMediaElement(MediaElement accentMediaElement, MediaElement normalMediaElement)
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
        //_metronomeService.Start(CurrentBPM, SelectedTimeSignature);
    }

    [RelayCommand]
    public void StopMetronome()
    {
        IsEnabled = true;
        _metronomeService.Stop();
    }
}