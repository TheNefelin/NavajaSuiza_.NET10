using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentSteelViewModel : InstrumentViewModelBase
{
    public InstrumentSteelViewModel(
        ILogger<InstrumentSteelViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetSteelStringConfig())
    {
    }
}
