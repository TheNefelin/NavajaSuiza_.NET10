using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentSteelPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public InstrumentSteelPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        TunerMediaElement.Stop();
        BindingContext = _serviceProvider.GetRequiredService<InstrumentSteelViewModel>();

        if (BindingContext is InstrumentSteelViewModel viewModel)
        {
            viewModel.RegisterMediaElement(TunerMediaElement);
            _ = viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        TunerMediaElement.Stop();
        
        if (BindingContext is InstrumentSteelViewModel viewModel)
        {
            _ = viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }
    }
}
