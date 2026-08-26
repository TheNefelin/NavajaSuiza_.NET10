using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentUkuleleViewModel : InstrumentViewModelBase
{
    public InstrumentUkuleleViewModel(
        ILogger<InstrumentUkuleleViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetUkeleleStringConfig())
    {
    }
}
