using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class FramingPage : ContentPage
{
	public FramingPage(FramingViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}