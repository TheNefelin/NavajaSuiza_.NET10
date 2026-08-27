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
    private async Task NavigateToNylon() => await _navigationService.PushAsync("InstrumentNylonPage");

    [RelayCommand]
    private async Task NavigateToSteel() => await _navigationService.PushAsync("InstrumentSteelPage");

    [RelayCommand]
    private async Task NavigateToBass() => await _navigationService.PushAsync("InstrumentBassPage");

    [RelayCommand]
    private async Task NavigateToUkelele() => await _navigationService.PushAsync("InstrumentUkulelePage");

    [RelayCommand]
    private async Task NavigateToViolin() => await _navigationService.PushAsync("InstrumentViolinPage");

    [RelayCommand]
    private async Task NavigateToCharango() => await _navigationService.PushAsync("InstrumentCharangoPage");
}
