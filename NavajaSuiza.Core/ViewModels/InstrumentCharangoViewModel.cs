using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentCharangoViewModel : InstrumentViewModelBase
{
    public InstrumentCharangoViewModel(
        ILogger<InstrumentCharangoViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetCharangoStringConfig())
    {
    }
}
