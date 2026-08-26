using CommunityToolkit.Mvvm.ComponentModel;

namespace NavajaSuiza.Core.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _title = string.Empty;

    public virtual void Cleanup()
    {
    }
}
