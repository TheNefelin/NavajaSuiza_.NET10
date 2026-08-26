using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentNylonViewModel : InstrumentViewModelBase
{
    public InstrumentNylonViewModel(
        ILogger<InstrumentNylonViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetNylonStringConfig())
    {
    }
}
