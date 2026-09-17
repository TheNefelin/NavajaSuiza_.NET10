using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class NotesPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private NotesViewModel? _viewModel;

    public NotesPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel ??= _serviceProvider.GetRequiredService<NotesViewModel>();
        BindingContext = _viewModel;

        if (_viewModel is not null)
        {
            await _viewModel.OnPageAppearingAsync();
        }
    }
}