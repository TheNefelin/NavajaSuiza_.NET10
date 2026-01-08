using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentCharangoPage : ContentPage
{
	public InstrumentCharangoPage(InstrumentCharangoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is InstrumentCharangoViewModel viewModel)
		{
			viewModel.RegisterMediaElement(TunerMediaElement);
			_ = viewModel.InitializeAsync();
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		if (BindingContext is InstrumentCharangoViewModel viewModel)
		{
			// Limpiar
			_ = viewModel.StopAllStringAsync();
			viewModel.ClearStringBorders();
        }
    }
}