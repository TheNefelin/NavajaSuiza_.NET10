using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class TestingPage : ContentPage
{
    public TestingPage(TestingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}