using Microsoft.Extensions.Logging;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class ManualViewModel : BaseViewModel
{
    private readonly ILogger<ManualViewModel> _logger;
    
    public ManualViewModel(
        ILogger<ManualViewModel> logger)
    {
        _logger = logger;
    }
}