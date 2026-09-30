using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<FlashlightViewModel> _logger;
    private readonly IFlashlightService _flashlightService;
    private readonly IDeviceDisplayService _deviceDisplayService;
    private readonly IFlashlightStateService _stateService;
    private readonly IMorseSignalService _morseSignalService;

    private CancellationTokenSource? _morseCts;
    private Task? _morseTask;

    private const int MinimumIntensityLevel = 1;

    public bool IsFlashOn
    {
        get => _stateService.IsFlashOn;
        set
        {
            if (_stateService.IsFlashOn != value)
            {
                _stateService.IsFlashOn = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    public partial bool IsScreenOn { get; set; }

    [ObservableProperty]
    public partial bool IsSosActive { get; set; }

    [ObservableProperty]
    public partial bool IsHelpActive { get; set; }

    public bool IsFlashIntensitySupported => _flashlightService.SupportsVariableIntensity;

    public bool IsFlashIntensityUnsupported => !IsFlashIntensitySupported;

    public int IntensityLevelMaximum => Math.Max(MinimumIntensityLevel, _flashlightService.MaxIntensityLevel);

    public int IntensityLevel
    {
        get => _flashlightService.IntensityLevel;
        set
        {
            if (_flashlightService.IntensityLevel == value)
                return;

            _flashlightService.IntensityLevel = value;
            OnPropertyChanged();
        }
    }

    public FlashlightViewModel(
        ILogger<FlashlightViewModel> logger,
        INavigationService navigationService,
        IFlashlightService flashlightService,
        IDeviceDisplayService deviceDisplayService,
        IFlashlightStateService stateService,
        IMorseSignalService morseSignalService)
    {
        _logger = logger;
        _navigationService = navigationService;
        _flashlightService = flashlightService;
        _deviceDisplayService = deviceDisplayService;
        _stateService = stateService;
        _morseSignalService = morseSignalService;
    }

    [RelayCommand]
    private async Task ClickFlash()
    {
        if (IsSosActive || IsHelpActive)
        {
            CancelMorse();
            await WaitMorseAsync();

            IsFlashOn = true;
            try
            {
                await _flashlightService.TurnOnAsync();
            }
            catch (Exception)
            {
                IsFlashOn = false;
            }

            return;
        }

        IsFlashOn = !IsFlashOn;

        try
        {
            if (IsFlashOn)
            {
                await _flashlightService.TurnOnAsync();
            }
            else
            {
                await _flashlightService.TurnOffAsync();
            }
        }
        catch (Exception)
        {
            IsFlashOn = !IsFlashOn;
        }
    }

    [RelayCommand]
    private async Task ClickScreen()
    {
        IsScreenOn = !IsScreenOn;

        try
        {
            if (IsScreenOn)
            {
                await _navigationService.PushAsync("ScreenLightPage");
                IsScreenOn = false;
            }
            else
            {
                _deviceDisplayService.KeepScreenOn = false;
            }
        }
        catch (Exception)
        {
            IsScreenOn = !IsScreenOn;
        }
    }

    [RelayCommand]
    private async Task SendSosAsync()
    {
        if (IsSosActive)
        {
            CancelMorse();
            await WaitMorseAsync();
            return;
        }

        await StartMorseAsync("SOS", MorseSignalSequence.Sos);
    }

    [RelayCommand]
    private async Task SendHelpAsync()
    {
        if (IsHelpActive)
        {
            CancelMorse();
            await WaitMorseAsync();
            return;
        }

        await StartMorseAsync("HELP", MorseSignalSequence.Help);
    }

    private async Task StartMorseAsync(string word, IReadOnlyList<MorseFrame> sequence)
    {
        if (IsSosActive || IsHelpActive)
        {
            CancelMorse();
            await WaitMorseAsync();
        }

        if (_stateService.IsFlashOn)
        {
            IsFlashOn = false;
            try
            {
                await _flashlightService.TurnOffAsync();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not turn the flash off before the Morse sequence");
            }
        }

        var cts = new CancellationTokenSource();
        _morseCts = cts;

        var loop = sequence
            .Concat(new[] { new MorseFrame(false, MorseSignalSequence.WORD_GAP_MS) })
            .ToList();

        SetMorseActive(word);
        _morseTask = RunMorseLoopAsync(loop, cts);
    }

    private void CancelMorse()
    {
        _morseCts?.Cancel();
    }

    private async Task WaitMorseAsync()
    {
        var task = _morseTask;
        _morseTask = null;

        if (task is null)
            return;

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetMorseActive(string word)
    {
        IsSosActive = word == "SOS";
        IsHelpActive = word == "HELP";
    }

    private async Task RunMorseLoopAsync(List<MorseFrame> loop, CancellationTokenSource cts)
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                await _morseSignalService.SendAsync(
                    loop,
                    isOn => isOn ? _flashlightService.TurnOnAsync() : _flashlightService.TurnOffAsync(),
                    cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            try
            {
                await _flashlightService.TurnOffAsync();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not turn the flash off after the Morse sequence");
            }

            if (ReferenceEquals(_morseCts, cts))
                _morseCts = null;

            SetMorseActive(string.Empty);
        }
    }
}