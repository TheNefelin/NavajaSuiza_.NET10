using Microsoft.Extensions.DependencyInjection;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class TextReaderPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public TextReaderPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var parameter = _serviceProvider.GetRequiredService<INavigationService>().TakeNavigationParameter();
        if (parameter is not string path || string.IsNullOrEmpty(path))
            return;

        var viewModel = _serviceProvider.GetRequiredService<TextReaderViewModel>();
        BindingContext = viewModel;
        viewModel.Load(path);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is TextReaderViewModel viewModel)
            viewModel.Unload();
    }
}