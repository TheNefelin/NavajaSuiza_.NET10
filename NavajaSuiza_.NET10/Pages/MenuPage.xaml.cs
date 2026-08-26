using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class MenuPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public MenuPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        BindingContext = _serviceProvider.GetRequiredService<MenuViewModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MenuViewModel viewModel)
        {
            viewModel.OnPageAppearing();
        }
    }
}
