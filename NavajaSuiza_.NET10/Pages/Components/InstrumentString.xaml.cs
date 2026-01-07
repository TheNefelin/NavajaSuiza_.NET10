using NavajaSuiza_.NET10.PagesViewModel;

namespace NavajaSuiza_.NET10.Pages.Components;

public partial class InstrumentString : ContentView
{
    public static readonly BindableProperty NoteProperty = BindableProperty.Create(nameof(Note), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty AudioNameProperty = BindableProperty.Create(nameof(AudioName), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(nameof(Thickness), typeof(string), typeof(InstrumentStringComponent), string.Empty);

    public string Note
    {
        get => (string)GetValue(NoteProperty);
        set => SetValue(NoteProperty, value);
    }

    public string AudioName
    {
        get => (string)GetValue(AudioNameProperty);
        set => SetValue(AudioNameProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Thickness
    {
        get => (string)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public InstrumentString()
	{
		InitializeComponent();
	}

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (this.BindingContext is TestingViewModel viewModel)
        {
            var internalBorder = this.FindByName<Border>("InternalStringBorder");

            if (internalBorder != null)
            {
                viewModel.RegisterStringBorder(internalBorder, AudioName);
            }
        }
    }

    private async void OnStringTapped(object sender, TappedEventArgs e)
    {
        if (this.BindingContext is TestingViewModel viewModel)
        {
            var internalBorder = this.FindByName<Border>("InternalStringBorder");

            if (internalBorder != null)
            {
                _ = viewModel.StringTappedAsync(internalBorder);
            }
        }
    }
}