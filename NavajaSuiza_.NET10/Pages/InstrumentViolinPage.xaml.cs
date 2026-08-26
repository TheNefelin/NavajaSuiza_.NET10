using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentViolinPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public InstrumentViolinPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<InstrumentViolinViewModel>();

        if (BindingContext is InstrumentViolinViewModel viewModel)
        {
            viewModel.RegisterMediaElement(TunerMediaElement);
            _ = viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is InstrumentViolinViewModel viewModel)
        {
            _ = viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }
    }
}
