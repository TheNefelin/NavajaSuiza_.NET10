using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class NoteEditorPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private NoteEditorViewModel? _viewModel;

    public NoteEditorPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        _viewModel ??= _serviceProvider.GetRequiredService<NoteEditorViewModel>();
        BindingContext = _viewModel;

        if (_viewModel is not null)
        {
            _viewModel.Initialize();
        }
    }
}