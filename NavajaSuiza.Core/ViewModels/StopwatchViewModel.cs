using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.ViewModels;

public partial class StopwatchViewModel : BaseViewModel
{
    private readonly ILogger<StopwatchViewModel> _logger;
    private readonly IStopwatchService _stopwatchService;

    public ObservableCollection<StopwatchLap> Laps { get; } = new();

    [ObservableProperty]
    public partial string ElapsedText { get; set; } = FormatTimeSpan(TimeSpan.Zero);

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    public StopwatchViewModel(
        ILogger<StopwatchViewModel> logger,
        IStopwatchService stopwatchService)
    {
        _logger = logger;
        _stopwatchService = stopwatchService;
    }

    public void Initialize()
    {
        _stopwatchService.Tick += OnTick;
        ElapsedText = FormatTimeSpan(_stopwatchService.Elapsed);
        IsRunning = _stopwatchService.IsRunning;
    }

    public override void Cleanup()
    {
        _stopwatchService.Tick -= OnTick;
        base.Cleanup();
    }

    private void OnTick(TimeSpan elapsed)
    {
        ElapsedText = FormatTimeSpan(elapsed);
    }

    [RelayCommand]
    private void StartLap()
    {
        if (_stopwatchService.IsRunning)
        {
            AddLap();
        }
        else
        {
            _stopwatchService.Start();
            _logger.LogInformation("Stopwatch started");
        }

        IsRunning = _stopwatchService.IsRunning;
    }

    [RelayCommand]
    private void StopReset()
    {
        if (_stopwatchService.IsRunning)
        {
            _stopwatchService.Pause();
            _logger.LogInformation("Stopwatch stopped");
        }
        else
        {
            _stopwatchService.Stop();
            Laps.Clear();
            ElapsedText = FormatTimeSpan(TimeSpan.Zero);
            _logger.LogInformation("Stopwatch reset");
        }

        IsRunning = _stopwatchService.IsRunning;
    }

    private void AddLap()
    {
        var split = _stopwatchService.Elapsed;
        var previous = Laps.Count > 0 ? Laps[0].Split : TimeSpan.Zero;

        Laps.Insert(0, new StopwatchLap
        {
            Number = Laps.Count + 1,
            Split = split,
            Delta = split - previous
        });

        _logger.LogInformation("Lap {Number} recorded at {Split}", Laps[0].Number, split);
    }

    public static string FormatTimeSpan(TimeSpan elapsed) => StopwatchLap.FormatTime(elapsed);
}