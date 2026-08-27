using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentNylonViewModel : InstrumentViewModelBase
{
    public InstrumentNylonViewModel(
        ILogger<InstrumentNylonViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetNylonStringConfig())
    {
    }
}
