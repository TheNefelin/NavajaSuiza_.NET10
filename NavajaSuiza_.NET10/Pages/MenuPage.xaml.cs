using NavajaSuiza_.NET10.Services.Implementations;
using System.Globalization;

namespace NavajaSuiza_.NET10.Pages;

public partial class MenuPage : ContentPage
{
    public LocalizationResourceManager LocalizationResourceManager
    => LocalizationResourceManager.Instance;

    public MenuPage()
	{
		InitializeComponent();
        BindingContext = this;
    }

    private void OnClicked(object sender, EventArgs e)
    {
        // Get current language code
        var currentLang = LocalizationResourceManager.Instance.Culture.TwoLetterISOLanguageName;

        // Cycle through: es -> en -> sv -> es
        var nextCulture = currentLang switch
        {
            "es" => new CultureInfo("en-US"),
            "en" => new CultureInfo("sv-SE"),
            _ => new CultureInfo("es-CL")
        };

        // Set new culture
        LocalizationResourceManager.Instance.Culture = nextCulture;

        // Update button text
        BtnTest.Text = LocalizationResourceManager.Instance["MenuText"].ToString();
    }
}