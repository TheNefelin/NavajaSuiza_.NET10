using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class NavigationService : INavigationService
{
    private readonly object _sync = new();
    private object? _lastParameter;

    public async Task PushAsync(string pageName, object? parameter = null)
    {
        lock (_sync)
        {
            _lastParameter = parameter;
        }

        var pageType = Type.GetType($"NavajaSuiza_.NET10.Pages.{pageName}, NavajaSuiza_.NET10");
        if (pageType is null)
            return;

        var page = Shell.Current.Handler?.MauiContext?.Services.GetService(pageType) as Page;
        if (page is not null)
            await Shell.Current.Navigation.PushAsync(page);
    }

    public async Task PopAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }

    public object? TakeNavigationParameter()
    {
        lock (_sync)
        {
            var parameter = _lastParameter;
            _lastParameter = null;
            return parameter;
        }
    }

    public async Task GoToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    public async Task DisplayAlertAsync(string title, string message, string accept)
    {
        await Shell.Current.DisplayAlertAsync(title, message, accept);
    }

    public async Task<bool> DisplayAlertConfirmAsync(string title, string message, string accept, string cancel)
    {
        return await Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
    }
}