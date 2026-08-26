using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentBassViewModel : InstrumentViewModelBase
{
    public InstrumentBassViewModel(
        ILogger<InstrumentBassViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetBassStringConfig())
    {
    }
}
