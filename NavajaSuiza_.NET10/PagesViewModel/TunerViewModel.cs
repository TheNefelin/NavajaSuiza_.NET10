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
    private async Task NavigateToViolin()
    {
        var page = _serviceProvider.GetRequiredService<InstrumentViolinPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }
}