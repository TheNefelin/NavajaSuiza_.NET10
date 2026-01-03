using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class FlashlightPage : ContentPage
{
	public FlashlightPage(FlashlightViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}