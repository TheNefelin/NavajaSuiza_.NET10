using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class FlashlightPage : ContentPage
{
	public FlashlightPage(FlashlightViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}