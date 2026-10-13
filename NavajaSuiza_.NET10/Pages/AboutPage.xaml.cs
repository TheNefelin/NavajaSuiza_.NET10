using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using NavajaSuiza_.NET10.Diagnostics;
using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class AboutPage : ContentPage
{
    private const string DiagnosticsMimeType = "text/plain";

    private readonly IServiceProvider _serviceProvider;
    private bool _isBusy;

    public AboutPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        BindingContext = _serviceProvider.GetRequiredService<AboutViewModel>();
    }

    private async void OnViewDiagnosticsClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _isBusy = true;
        try
        {
            var path = DiagnosticsPaths.LogFilePath;
            if (!await EnsureLogExistsAsync(path))
                return;

            await _serviceProvider.GetRequiredService<INavigationService>().PushAsync("TextReaderPage", path);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnShareDiagnosticsClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _isBusy = true;
        try
        {
            var path = DiagnosticsPaths.LogFilePath;
            if (!await EnsureLogExistsAsync(path))
                return;

            try
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = LocalizationResourceManager.Instance["AboutShareDiagnosticsText"]?.ToString() ?? string.Empty,
                    File = new ShareFile(path, DiagnosticsMimeType)
                });
            }
            catch (Exception)
            {
                // Compartir el diagnóstico nunca debe tumbar la aplicación.
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnSaveDiagnosticsClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _isBusy = true;
        try
        {
            var local = LocalizationResourceManager.Instance;

            var path = DiagnosticsPaths.LogFilePath;
            if (!await EnsureLogExistsAsync(path))
                return;

            try
            {
                using var stream = File.OpenRead(path);
                var result = await FileSaver.Default.SaveAsync(Path.GetFileName(path), stream, CancellationToken.None);

                if (result.IsSuccessful || result.IsCancelled)
                    return;

                await DisplayAlertAsync(
                    local["DiagnosticsSaveErrorTitleText"]?.ToString() ?? string.Empty,
                    local["DiagnosticsSaveErrorText"]?.ToString() ?? string.Empty,
                    local["CommonOkText"]?.ToString() ?? string.Empty);
            }
            catch (Exception)
            {
                // Guardar el diagnóstico nunca debe tumbar la aplicación.
            }
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task<bool> EnsureLogExistsAsync(string path)
    {
        if (File.Exists(path))
            return true;

        var local = LocalizationResourceManager.Instance;
        await DisplayAlertAsync(
            local["DiagnosticsNoLogTitleText"]?.ToString() ?? string.Empty,
            local["DiagnosticsNoLogText"]?.ToString() ?? string.Empty,
            local["CommonOkText"]?.ToString() ?? string.Empty);
        return false;
    }
}
