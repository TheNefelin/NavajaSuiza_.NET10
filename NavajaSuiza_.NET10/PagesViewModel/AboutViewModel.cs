using CommunityToolkit.Mvvm.ComponentModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class AboutViewModel : BaseViewModel
{
    [ObservableProperty]
    private List<string> themeOptions = new() { "Auto", "Light", "Dark" };

    [ObservableProperty]
    private string selectedTheme = "Auto";

    public AboutViewModel()
    {
        Title = "About";
    }
}
