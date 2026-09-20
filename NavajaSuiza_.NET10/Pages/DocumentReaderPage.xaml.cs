using Microsoft.Extensions.DependencyInjection;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class DocumentReaderPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public DocumentReaderPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        BindingContext = _serviceProvider.GetRequiredService<DocumentReaderViewModel>();
    }
}