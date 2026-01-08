using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentSteelPage : ContentPage
{
	public InstrumentSteelPage(InstrumentSteelViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is InstrumentSteelViewModel viewModel)
        {
            viewModel.RegisterMediaElement(TunerMediaElement);
            _ = viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is InstrumentSteelViewModel viewModel)
        {
            // Limpiar
            _ = viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }
    }
}