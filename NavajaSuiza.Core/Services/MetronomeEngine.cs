using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.Services;

/// <summary>
/// Dueña de la sesión del metrónomo. No programa latidos: eso vive en el
/// <see cref="IMetronomePlayer"/> de cada plataforma. Su responsabilidad es una sola y crítica:
/// garantizar que en todo momento exista <b>como máximo una</b> sesión de reproducción.
///
/// Antes esta lógica convivía con dos campos sueltos (<c>_cts</c> y <c>_timerTask</c>) que dos
/// llamadas concurrentes podían sobrescribir, dejando bucles huérfanos que nadie cancelaba y que
/// seguían sonando. Aquí el semáforo serializa start/stop y cada <c>Start</c> va precedido de un
/// <c>Stop</c>, con lo que el estado de la sesión es siempre inequívoco.
/// </summary>
public sealed class MetronomeEngine
{
    private readonly IMetronomePlayer _player;
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private readonly object _stateLock = new();

    private int _bpm = AppConstants.Metronome.DEFAULT_BPM;
    private int _beatsPerMeasure = MetronomeTempo.BeatsPerMeasure(AppConstants.Metronome.DEFAULT_TIME_SIGNATURE);
    private bool _isRunning;

    public MetronomeEngine(IMetronomePlayer player)
    {
        _player = player;
    }

    public int Bpm
    {
        get { lock (_stateLock) return _bpm; }
    }

    public int BeatsPerMeasure
    {
        get { lock (_stateLock) return _beatsPerMeasure; }
    }

    public bool IsRunning
    {
        get { lock (_stateLock) return _isRunning; }
    }

    public async Task StartAsync(int bpm, int beatsPerMeasure)
    {
        await _lifecycle.WaitAsync().ConfigureAwait(false);

        try
        {
            int effectiveBpm, effectiveBeats;

            lock (_stateLock)
            {
                _bpm = MetronomeTempo.Clamp(bpm);
                _beatsPerMeasure = Math.Max(1, beatsPerMeasure);
                effectiveBpm = _bpm;
                effectiveBeats = _beatsPerMeasure;
            }

            _player.Stop();
            _player.Start(effectiveBpm, effectiveBeats);

            lock (_stateLock)
                _isRunning = true;
        }
        finally
        {
            _lifecycle.Release();
        }
    }

    public async Task StopAsync()
    {
        await _lifecycle.WaitAsync().ConfigureAwait(false);

        try
        {
            _player.Stop();

            lock (_stateLock)
                _isRunning = false;
        }
        finally
        {
            _lifecycle.Release();
        }
    }

    public void SetTempo(int bpm)
    {
        int effectiveBpm;

        lock (_stateLock)
        {
            _bpm = MetronomeTempo.Clamp(bpm);
            effectiveBpm = _bpm;
        }

        _player.SetTempo(effectiveBpm);
    }

    public void SetTimeSignature(int beatsPerMeasure)
    {
        int effectiveBeats;

        lock (_stateLock)
        {
            _beatsPerMeasure = Math.Max(1, beatsPerMeasure);
            effectiveBeats = _beatsPerMeasure;
        }

        _player.SetTimeSignature(effectiveBeats);
    }
}