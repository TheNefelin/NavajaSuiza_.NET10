using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class MetronomePage : ContentPage
{
	public MetronomePage(MetronomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MetronomeViewModel viewModel)
        {
            viewModel.RegisterMediaElement(AccentMediaElement, NormalMediaElement);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is MetronomeViewModel viewModel)
        {
            viewModel.StopMetronome();
        }
    }
}