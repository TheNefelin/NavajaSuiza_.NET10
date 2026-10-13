using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza.Core.ViewModels;

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
            _ = viewModel.StopMetronome();
        }
    }

    private async void OnBpmTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not MetronomeViewModel viewModel)
            return;

        var local = LocalizationResourceManager.Instance;
        var title = local["MetronomeBpmPromptTitleText"]?.ToString() ?? string.Empty;
        var message = local["MetronomeBpmPromptMessageText"]?.ToString() ?? string.Empty;
        var accept = local["CommonOkText"]?.ToString() ?? string.Empty;
        var cancel = local["CancelText"]?.ToString() ?? string.Empty;

        var input = await DisplayPromptAsync(
            title,
            message,
            accept,
            cancel,
            keyboard: Keyboard.Numeric,
            maxLength: 3,
            initialValue: viewModel.CurrentBPM.ToString());

        if (int.TryParse(input, out var bpm))
        {
            viewModel.SetBpm(bpm);
        }
    }
}
