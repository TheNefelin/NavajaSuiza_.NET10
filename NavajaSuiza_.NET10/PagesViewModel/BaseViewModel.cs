using CommunityToolkit.Mvvm.ComponentModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

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
        // Método virtual para que las clases derivadas lo implementen si lo necesitan
    }
}
