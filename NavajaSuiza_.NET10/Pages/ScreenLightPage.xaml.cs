using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class ScreenLightPage : ContentPage
{
	public ScreenLightPage(ScreenLightViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ScreenLightViewModel viewModel)
        {
            viewModel.InitializeScreenLight();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is ScreenLightViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}