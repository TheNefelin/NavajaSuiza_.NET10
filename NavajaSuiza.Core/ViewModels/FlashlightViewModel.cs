using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza.Core.ViewModels;

public partial class FlashlightViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ILanguageService _languageService;
    private readonly IFlashlightService _flashlightService;
    private readonly IDeviceDisplayService _deviceDisplayService;
    private readonly IFlashlightStateService _stateService;
    private readonly IMorseSignalService _morseSignalService;

    private CancellationTokenSource? _morseCts;
    private Task? _morseTask;

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
    public partial bool IsLightOn { get; set; }

    [ObservableProperty]
    public partial bool IsSosActive { get; set; }

    [ObservableProperty]
    public partial bool IsHelpActive { get; set; }

    public FlashlightViewModel(
        INavigationService navigationService,
        ILanguageService languageService,
        IFlashlightService flashlightService,
        IDeviceDisplayService deviceDisplayService,
        IFlashlightStateService stateService,
        IMorseSignalService morseSignalService)
    {
        _navigationService = navigationService;
        _languageService = languageService;
        _flashlightService = flashlightService;
        _deviceDisplayService = deviceDisplayService;
        _stateService = stateService;
        _morseSignalService = morseSignalService;
        _languageService.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsLightOn));
    }

    public override void Cleanup()
    {
        _languageService.LanguageChanged -= OnLanguageChanged;
        base.Cleanup();
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
            catch (Exception)
            {
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
            catch (Exception)
            {
            }

            if (ReferenceEquals(_morseCts, cts))
                _morseCts = null;

            SetMorseActive(string.Empty);
        }
    }
}