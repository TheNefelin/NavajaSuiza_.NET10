using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.ViewModels;

public partial class ManualViewModel : BaseViewModel
{
    private readonly ILogger<ManualViewModel> _logger;
    
    public ManualViewModel(
        ILogger<ManualViewModel> logger)
    {
        _logger = logger;
    }
}