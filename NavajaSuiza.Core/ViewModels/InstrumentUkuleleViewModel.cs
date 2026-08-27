using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentUkuleleViewModel : InstrumentViewModelBase
{
    public InstrumentUkuleleViewModel(
        ILogger<InstrumentUkuleleViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetUkeleleStringConfig())
    {
    }
}
