using NavajaSuiza.Core.ViewModels;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class FramingPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public FramingPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<FramingViewModel>();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100);

            if (BindingContext is FramingViewModel viewModel)
            {
                viewModel.SetCanvasWidth(PreviewFrame.Width);
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is FramingViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
