using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class ScreenLightPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public ScreenLightPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<ScreenLightViewModel>();

        if (BindingContext is ScreenLightViewModel viewModel)
        {
            viewModel.InitializeScreenLight();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ScreenLightViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
