using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class TunerPage : ContentPage
{
	public TunerPage(TunerViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}