using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class InstrumentSteelViewModel : InstrumentViewModelBase
{
    public InstrumentSteelViewModel(
        ILogger<InstrumentSteelViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetSteelStringConfig())
    {
    }
}
