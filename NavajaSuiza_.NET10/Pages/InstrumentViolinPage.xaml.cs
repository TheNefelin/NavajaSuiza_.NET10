using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentViolinPage : ContentPage
{
	public InstrumentViolinPage(InstrumentViolinViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
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
            // Limpiar
            _ =  viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }
    }
}