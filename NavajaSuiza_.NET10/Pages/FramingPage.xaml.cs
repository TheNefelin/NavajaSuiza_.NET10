using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class FramingPage : ContentPage
{
	public FramingPage(FramingViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100); // Pequeño delay para que se renderice

            if (BindingContext is FramingViewModel viewModel)
            {
                viewModel.SetCanvasWidth(PreviewFrame.Width);
            }
        });
    }
}