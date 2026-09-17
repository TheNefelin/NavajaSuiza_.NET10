namespace NavajaSuiza.Core.Interfaces;

public interface INavigationService
{
    Task PushAsync(string pageName, object? parameter = null);
    Task PopAsync();
    object? TakeNavigationParameter();
    Task GoToAsync(string route);
    Task DisplayAlertAsync(string title, string message, string accept);
    Task<bool> DisplayAlertConfirmAsync(string title, string message, string accept, string cancel);
}