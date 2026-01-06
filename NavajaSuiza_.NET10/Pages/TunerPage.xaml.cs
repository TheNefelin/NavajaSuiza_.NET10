using CommunityToolkit.Maui.Views;
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
            // Registrar el MediaElement del afinador
            viewModel.RegisterTunerMediaElement(TunerMediaElement);

            // Registrar cada cuerda con su Border
            // Guitar Standard Tuning: E2 A2 D3 G3 B3 E4
            viewModel.RegisterStringBorder("GN_E2", StringE2Border);
            viewModel.RegisterStringBorder("GN_A2", StringA2Border);
            viewModel.RegisterStringBorder("GN_D3", StringD3Border);
            viewModel.RegisterStringBorder("GN_G3", StringG3Border);
            viewModel.RegisterStringBorder("GN_B3", StringB3Border);
            viewModel.RegisterStringBorder("GN_E4", StringE4Border);
        }
    }
}