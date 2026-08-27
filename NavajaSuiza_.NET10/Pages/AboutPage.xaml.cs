using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class AboutPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public AboutPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        BindingContext = _serviceProvider.GetRequiredService<AboutViewModel>();
    }
}
