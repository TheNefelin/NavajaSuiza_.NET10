using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.ViewModels;

public partial class ManualViewModel : BaseViewModel
{
    private readonly ILogger<ManualViewModel> _logger;

    public ManualViewModel(
        ILogger<ManualViewModel> logger)
    {
        _logger = logger;
    }
}
