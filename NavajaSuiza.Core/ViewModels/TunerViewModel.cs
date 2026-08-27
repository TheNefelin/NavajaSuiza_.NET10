using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class TunerViewModel : BaseViewModel
{
    private readonly ILogger<TunerViewModel> _logger;
    private readonly INavigationService _navigationService;

    public TunerViewModel(
        ILogger<TunerViewModel> logger,
        INavigationService navigationService)
    {
        _logger = logger;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task NavigateToNylon()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentNylonPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateToSteel()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentSteelPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateToBass()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentBassPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateToUkelele()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentUkulelePage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateToViolin()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentViolinPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task NavigateToCharango()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await _navigationService.PushAsync("InstrumentCharangoPage"); }
        finally { IsBusy = false; }
    }
}
