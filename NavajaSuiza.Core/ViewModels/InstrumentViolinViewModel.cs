using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentViolinViewModel : InstrumentViewModelBase
{
    public InstrumentViolinViewModel(
        ILogger<InstrumentViolinViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetViolinStringConfig())
    {
    }
}
