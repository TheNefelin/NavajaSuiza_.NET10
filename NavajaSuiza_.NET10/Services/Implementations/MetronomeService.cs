using NavajaSuiza.Core;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza_.NET10.Services.Implementations;

/// <summary>
/// Fachada del metrónomo: persistencia de preferencias y traducción de firma de tiempos. No programa
/// latidos ni reproduce audio; de eso se ocupan <see cref="MetronomeEngine"/> y el
/// <see cref="IMetronomePlayer"/> de cada plataforma.
/// </summary>
public sealed class MetronomeService : IMetronomeService
{
    private const string BPM_KEY = "metronome_bpm";
    private const string TIME_SIGNATURE_KEY = "metronome_time_signature";

    private readonly IMetronomePlayer _player;
    private readonly MetronomeEngine _engine;

    public MetronomeService(IMetronomePlayer player)
    {
        _player = player;
        _engine = new MetronomeEngine(player);
    }

    public int SavedBpm => Preferences.Default.Get(BPM_KEY, AppConstants.Metronome.DEFAULT_BPM);

    public string SavedTimeSignature => Preferences.Default.Get(TIME_SIGNATURE_KEY, AppConstants.Metronome.DEFAULT_TIME_SIGNATURE);

    public void SetMediaElement(object accentMediaElement, object normalMediaElement) =>
        _player.AttachMediaSinks(accentMediaElement, normalMediaElement);

    public Task StartAsync(int currentBPM, string selectedTimeSignature) =>
        _engine.StartAsync(currentBPM, MetronomeTempo.BeatsPerMeasure(selectedTimeSignature));

    public Task StopAsync() => _engine.StopAsync();

    public void SetTempo(int currentBPM)
    {
        Preferences.Default.Set(BPM_KEY, MetronomeTempo.Clamp(currentBPM));
        _engine.SetTempo(currentBPM);
    }

    public void SetTimeSignature(string selectedTimeSignature)
    {
        Preferences.Default.Set(TIME_SIGNATURE_KEY, selectedTimeSignature);
        _engine.SetTimeSignature(MetronomeTempo.BeatsPerMeasure(selectedTimeSignature));
    }
}