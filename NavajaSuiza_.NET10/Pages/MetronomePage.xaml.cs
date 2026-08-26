using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class MetronomePage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public MetronomePage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<MetronomeViewModel>();

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
