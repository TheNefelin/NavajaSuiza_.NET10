using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class CompassPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private CompassViewModel? _viewModel;

    public CompassPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_viewModel is null)
        {
            _viewModel = _serviceProvider.GetRequiredService<CompassViewModel>();
            BindingContext = _viewModel;
        }

        _ = StartSensorsSafeAsync();
    }

    private async Task StartSensorsSafeAsync()
    {
        if (_viewModel is null) return;
        try
        {
            await _viewModel.StartSensorsAsync();
        }
        catch (Exception)
        {
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        try
        {
            _viewModel?.StopSensors();
        }
        catch (Exception)
        {
        }
    }
}
