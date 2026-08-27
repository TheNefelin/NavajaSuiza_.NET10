using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class NavigationService : INavigationService
{
    public async Task PushAsync(string pageName)
    {
        var page = Shell.Current.Handler.MauiContext?.Services.GetService(
            Type.GetType($"NavajaSuiza_.NET10.Pages.{pageName}, NavajaSuiza_.NET10"))
            as Page;

        if (page != null)
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
