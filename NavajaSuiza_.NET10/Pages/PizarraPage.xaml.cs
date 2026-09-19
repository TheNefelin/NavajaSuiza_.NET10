using Microsoft.Maui.Graphics;
using NavajaSuiza_.NET10.Components;
using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class PizarraPage : ContentPage
{
    private const string LightBoardHex = "#F2F2EE";
    private const string DarkBoardHex = "#17171B";

    private readonly IServiceProvider _serviceProvider;
    private PizarraViewModel? _viewModel;
    private StrokeDrawable? _drawable;

    public PizarraPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var viewModel = _serviceProvider.GetRequiredService<PizarraViewModel>();
        _viewModel = viewModel;
        viewModel.BoardColor = IsDarkThemeActive() ? DarkBoardHex : LightBoardHex;

        _drawable = new StrokeDrawable(viewModel);
        DrawingSurface.Drawable = _drawable;
        BindingContext = viewModel;

        viewModel.CanvasChanged += OnCanvasChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_viewModel is not null)
        {
            _viewModel.CanvasChanged -= OnCanvasChanged;
            _viewModel.Cleanup();
        }
    }

    private void OnCanvasChanged() => DrawingSurface.Invalidate();

    private void OnBoardClicked(object? sender, EventArgs e)
    {
        _ = ShowBoardPickerAsync();
    }

    private void OnColorPickerScrimTapped(object? sender, TappedEventArgs e)
    {
        _viewModel?.CloseColorPickerCommand.Execute(null);
    }

    private async Task ShowBoardPickerAsync()
    {
        if (_viewModel is null)
            return;

        var title = LocalizationResourceManager.Instance["PizarraText"]?.ToString() ?? string.Empty;
        var cancelText = LocalizationResourceManager.Instance["CancelText"]?.ToString() ?? string.Empty;
        var boardLightText = LocalizationResourceManager.Instance["PizarraBoardLightText"]?.ToString() ?? string.Empty;
        var boardDarkText = LocalizationResourceManager.Instance["PizarraBoardDarkText"]?.ToString() ?? string.Empty;

        var result = await DisplayActionSheetAsync(title, cancelText, null, boardLightText, boardDarkText);

        if (string.IsNullOrEmpty(result) || result == cancelText)
            return;

        var hex = result == boardLightText ? LightBoardHex : DarkBoardHex;
        _viewModel.SetBoardColorCommand.Execute(hex);
    }

    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        var viewModel = _viewModel;
        var touches = e.Touches;
        if (viewModel is null || touches.Length == 0)
            return;

        var touch = touches[0];
        viewModel.StartStroke(touch.X, touch.Y);
    }

    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        var viewModel = _viewModel;
        var touches = e.Touches;
        if (viewModel is null || touches.Length == 0)
            return;

        var touch = touches[0];
        viewModel.AddPoint(touch.X, touch.Y);
    }

    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        _viewModel?.EndStroke();
    }

    private void OnCancelInteraction(object? sender, TouchEventArgs e)
    {
        _viewModel?.EndStroke();
    }

    private static bool IsDarkThemeActive() =>
        Application.Current?.RequestedTheme == AppTheme.Dark;
}