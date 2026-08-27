using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentBassPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public InstrumentBassPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        TunerMediaElement.Stop();
        BindingContext = _serviceProvider.GetRequiredService<InstrumentBassViewModel>();

        if (BindingContext is InstrumentBassViewModel viewModel)
        {
            viewModel.RegisterMediaElement(TunerMediaElement);
            _ = viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        TunerMediaElement.Stop();
        
        if (BindingContext is InstrumentBassViewModel viewModel)
        {
            _ = viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }
    }
}