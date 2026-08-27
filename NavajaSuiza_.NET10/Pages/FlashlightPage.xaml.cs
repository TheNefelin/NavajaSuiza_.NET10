using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class FlashlightPage : ContentPage
{
    public FlashlightPage(FlashlightViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
