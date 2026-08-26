using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.Pages.Components;

public partial class InstrumentStringComponent : ContentView
{
    private IInstrumentAudioService? _instrumentAudioService;

    public static readonly BindableProperty NoteProperty = BindableProperty.Create(nameof(Note), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty AudioNameProperty = BindableProperty.Create(nameof(AudioName), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty ThicknessProperty = BindableProperty.Create(nameof(Thickness), typeof(string), typeof(InstrumentStringComponent), string.Empty);
    public static readonly BindableProperty AudioServiceProperty = BindableProperty.Create(nameof(AudioService), typeof(IInstrumentAudioService), typeof(InstrumentStringComponent), null, propertyChanged: OnAudioServiceChanged);

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

    public IInstrumentAudioService AudioService
    {
        get => (IInstrumentAudioService)GetValue(AudioServiceProperty);
        set => SetValue(AudioServiceProperty, value);
    }

    public InstrumentStringComponent()
	{
		InitializeComponent();
    }

    private static void OnAudioServiceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InstrumentStringComponent component && newValue is IInstrumentAudioService service)
        {
            component._instrumentAudioService = service;
            component.RegisterBorder();
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        RegisterBorder();
    }

    private void RegisterBorder()
    {
        var internalBorder = this.FindByName<Border>("InternalStringBorder");
        if (internalBorder != null && _instrumentAudioService != null)
        {
            _instrumentAudioService.RegisterStringBorder(internalBorder, AudioName);
        }
    }

    private async void OnStringTapped(object sender, TappedEventArgs e)
    {
        if (_instrumentAudioService != null)
        {
            var internalBorder = this.FindByName<Border>("InternalStringBorder");
            if (internalBorder != null)
            {
                await _instrumentAudioService.StringTappedAsync(internalBorder);
            }
        }
    }
}