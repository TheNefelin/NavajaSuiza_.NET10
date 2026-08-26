using NavajaSuiza_.NET10.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class TestingPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public TestingPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<TestingViewModel>();
    }
}
