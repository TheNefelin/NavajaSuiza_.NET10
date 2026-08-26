using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class FlashlightPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public FlashlightPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<FlashlightViewModel>();
    }
}
