using Microsoft.Extensions.Logging;
using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class InstrumentCharangoViewModel : InstrumentViewModelBase
{
    public InstrumentCharangoViewModel(
        ILogger<InstrumentCharangoViewModel> logger,
        IInstrumentAudioService instrumentAudioService)
        : base(logger, instrumentAudioService, instrumentAudioService.GetCharangoStringConfig())
    {
    }
}
