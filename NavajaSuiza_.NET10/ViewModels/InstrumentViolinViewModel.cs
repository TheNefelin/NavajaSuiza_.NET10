using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentViolinViewModel : InstrumentViewModelBase
{
    public InstrumentViolinViewModel(
        ILogger<InstrumentViolinViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetViolinStringConfig())
    {
    }
}
