using CommunityToolkit.Mvvm.Input;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class MenuViewModel : BaseViewModel
{
    [RelayCommand]
    private async Task NavigateToFlashlight()
    {
        //await AppShell.DisplayToastAsync("All cleaned up!");
        await Shell.Current.GoToAsync("//flashlight");
    }
}
