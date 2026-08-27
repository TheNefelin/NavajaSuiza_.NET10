using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class NavigationService : INavigationService
{
    public async Task PushAsync(string pageName)
    {
        var pageType = Type.GetType($"NavajaSuiza_.NET10.Pages.{pageName}, NavajaSuiza_.NET10");
        if (pageType is null)
            return;

        var page = Shell.Current.Handler?.MauiContext?.Services.GetService(pageType) as Page;
        if (page is not null)
            await Shell.Current.Navigation.PushAsync(page);
    }

    public async Task GoToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    public async Task DisplayAlertAsync(string title, string message, string accept)
    {
        await Shell.Current.DisplayAlertAsync(title, message, accept);
    }
}
