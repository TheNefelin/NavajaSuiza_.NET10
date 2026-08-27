using NavajaSuiza.Core.ViewModels;

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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is FlashlightViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
