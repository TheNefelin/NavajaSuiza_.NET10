using Microsoft.Extensions.DependencyInjection;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class DocumentReaderPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private DocumentReaderViewModel? _viewModel;

    public DocumentReaderPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel = _serviceProvider.GetRequiredService<DocumentReaderViewModel>();
        BindingContext = _viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PdfViewer.UnloadDocument();
        _viewModel?.Unload();
    }
}