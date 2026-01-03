using CommunityToolkit.Mvvm.Input;
using NavajaSuiza_.NET10.Pages;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class MenuViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    public MenuViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task NavigateToFlashlight()
    {
        var page = _serviceProvider.GetRequiredService<FlashlightPage>();
        await Shell.Current.Navigation.PushAsync(page);
    }
}
