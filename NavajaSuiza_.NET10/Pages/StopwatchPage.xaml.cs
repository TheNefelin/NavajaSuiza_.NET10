using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class StopwatchPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public StopwatchPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<StopwatchViewModel>();

        if (BindingContext is StopwatchViewModel viewModel)
        {
            viewModel.Initialize();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is StopwatchViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}