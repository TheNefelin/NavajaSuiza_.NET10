using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class TestingViewModel : BaseViewModel
{
    private readonly ILogger<TestingViewModel> _logger;
    private readonly IInstrumentAudioService _instrumentAudioService;

    public TestingViewModel(
        ILogger<TestingViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
    {
        _logger = logger;
        _instrumentAudioService = instrumentAudioService;
    }

    public async void RegisterMediaElement(MediaElement mediaElement)
    {
        await Task.Run(() => {
            _logger.LogInformation("[TestingViewModel] - Registering MediaElement in TestingViewModel");
            _instrumentAudioService.RegisterMediaElement(mediaElement);
        });
    }

    public void RegisterStringBorder(Border border, string audioName)
    {
        _logger.LogInformation("[TestingViewModel] - Registering String Border in TestingViewModel");
        _instrumentAudioService.RegisterStringBorder(border, audioName);
    }

    public async Task StringTappedAsync(Border border)
    {
        _logger.LogInformation("[TestingViewModel] - String Tapped in TestingViewModel");   
        await _instrumentAudioService.StringTappedAsync(border);
    }

    [RelayCommand]
    public async Task StopAllStringAsync()
    {
        _logger.LogInformation("[TestingViewModel] - Stopping All Strings in TestingViewModel");    
        await _instrumentAudioService.StopAllStringAsync();
    }
}