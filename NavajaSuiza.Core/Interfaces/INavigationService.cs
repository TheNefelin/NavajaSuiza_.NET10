namespace NavajaSuiza.Core.Interfaces;

public interface INavigationService
{
    Task PushAsync(string pageName);
    Task GoToAsync(string route);
    Task DisplayAlertAsync(string title, string message, string accept);
}
