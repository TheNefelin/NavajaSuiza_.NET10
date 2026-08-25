using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class ManualPage : ContentPage
{
	public ManualPage(ManualViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}