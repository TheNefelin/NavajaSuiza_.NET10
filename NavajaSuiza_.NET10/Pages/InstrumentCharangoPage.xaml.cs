using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class InstrumentCharangoPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private IInstrumentAudioService? _audioService;

    public InstrumentCharangoPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _audioService = _serviceProvider.GetService<IInstrumentAudioService>();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<InstrumentCharangoViewModel>();

        if (BindingContext is InstrumentCharangoViewModel viewModel)
        {
            viewModel.RegisterMediaElement(TunerMediaElement);
            _ = viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        TunerMediaElement.Stop();
        
        if (BindingContext is InstrumentCharangoViewModel viewModel)
        {
            _ = viewModel.StopAllStringAsync();
            viewModel.ClearStringBorders();
        }

        if (_audioService != null)
        {
            _audioService.StopAllStringAsync();
        }
    }
}
