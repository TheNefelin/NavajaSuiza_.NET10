using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class CompassPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public CompassPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<CompassViewModel>();

        if (BindingContext is CompassViewModel viewModel)
        {
            viewModel.StartSensorsAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is CompassViewModel viewModel)
        {
            viewModel.StopSensors();
        }
    }
}
