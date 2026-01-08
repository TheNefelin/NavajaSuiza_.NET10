using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Pages;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class TunerViewModel : BaseViewModel
{
    private readonly ILogger<TunerViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TunerViewModel(
        ILogger<TunerViewModel> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task NavigateToNylon()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentNylonPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToSteel()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentSteelPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToBass()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentBassPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToUkelele()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentUkulelePage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToViolin()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentViolinPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }

    [RelayCommand]
    private async Task NavigateToCharango()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentCharangoPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }
}