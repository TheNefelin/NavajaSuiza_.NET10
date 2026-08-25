using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}