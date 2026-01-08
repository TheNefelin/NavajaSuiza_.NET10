using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentBassPage : ContentPage
{
	public InstrumentBassPage(InstrumentBassViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is InstrumentBassViewModel viewModel)
		{
			viewModel.RegisterMediaElement(TunerMediaElement);
			_ = viewModel.InitializeAsync();
		}
	}
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (BindingContext is InstrumentBassViewModel viewModel)
		{
			// Limpiar
			_ = viewModel.StopAllStringAsync();
			viewModel.ClearStringBorders();
        }
    }
}