using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages;

public partial class TestingPage : ContentPage
{
    public TestingPage(TestingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TestingViewModel viewModel)
        {
            // Registrar el MediaElement del afinador
            viewModel.RegisterMediaElement(TunerMediaElement);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is TestingViewModel viewModel)
        {
            // Detener cualquier reproducción en curso al salir de la página
            _ = viewModel.StopAllStringAsync();
        }
    }
}