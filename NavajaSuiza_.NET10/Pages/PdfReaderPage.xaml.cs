using Microsoft.Extensions.DependencyInjection;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class PdfReaderPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public PdfReaderPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var parameter = _serviceProvider.GetRequiredService<INavigationService>().TakeNavigationParameter();
        if (parameter is not PdfReaderPayload payload || string.IsNullOrEmpty(payload.FileName))
            return;

        var viewModel = _serviceProvider.GetRequiredService<PdfReaderViewModel>();
        BindingContext = viewModel;

        if (!string.IsNullOrEmpty(payload.Path))
            viewModel.Load(payload.Path);
        else if (payload.Stream is not null)
            viewModel.Load(payload.Stream, payload.FileName);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PdfViewer.UnloadDocument();
        if (BindingContext is PdfReaderViewModel viewModel)
            viewModel.Unload();
    }
}