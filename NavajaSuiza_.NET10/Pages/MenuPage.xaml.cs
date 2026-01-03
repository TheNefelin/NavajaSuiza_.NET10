using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class MenuPage : ContentPage
{
    public MenuPage(MenuViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}