using CommunityToolkit.Mvvm.ComponentModel;

namespace NavajaSuiza.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    public virtual void Cleanup()
    {
    }
}
