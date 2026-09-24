using CommunityToolkit.Mvvm.ComponentModel;

namespace NavajaSuiza.Core.ViewModels;

public partial class GuideViewModel : BaseViewModel
{
    [ObservableProperty]
    public partial string HtmlContent { get; set; } = string.Empty;
}