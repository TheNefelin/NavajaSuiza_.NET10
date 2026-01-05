using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class TunerPage : ContentPage
{
	public TunerPage(TunerViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TunerViewModel viewModel)
        {
            // Registrar cada cuerda con su Border
            viewModel.RegisterStringBorder("E2", StringE2Border);
            viewModel.RegisterStringBorder("A2", StringA2Border);
            viewModel.RegisterStringBorder("D3", StringD3Border);
            viewModel.RegisterStringBorder("G3", StringG3Border);
            viewModel.RegisterStringBorder("B3", StringB3Border);
            viewModel.RegisterStringBorder("E4", StringE4Border);
        }
    }
}