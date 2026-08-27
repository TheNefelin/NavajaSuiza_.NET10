using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentBassViewModel : InstrumentViewModelBase
{
    public InstrumentBassViewModel(
        ILogger<InstrumentBassViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetBassStringConfig())
    {
    }
}
