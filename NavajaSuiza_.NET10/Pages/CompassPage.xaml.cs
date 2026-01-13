using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class CompassPage : ContentPage
{
	public CompassPage(CompassViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

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