# NavajaSuiza .NET10

### Dependencies
- CommunityToolkit.Maui 13.0.0
- CommunityToolkit.Maui.MediaElement 6.1.3
- CommunityToolkit.Mvvm 8.4.0
- Microsoft.Extensions.Logging.Debug 10.0.1
- Microsoft.Maui.Controls 10.0.20
- Syncfusion.Maui.Toolkit 1.0.8

- SkiaSharp.Views.Maui.Controls 3.119.1
- SkiaSharp.Extended 3.0.0


### AndroidManifest permissions
- BATTERY_STATUS
- CAMERA
- FLASHLIGHT
- ACCESS_COARSE_LOCATION
- ACCESS_FINE_LOCATION

### Structure
```
NavajaSuiza_.NET10/
│
├── 📁 Behaviors/
│   └── ???
│
├── 📁 Converters/
│   ├── InvertedBoolConverter.cs
│   └── StringNotEmptyToBoolConverter.cs
│
├── 📁 Extensions/
│   ├── LocalizationResourceManager.cs
│   └── TranslateExtension.cs
│
├── 📁 Models/
│   └── SupportedLanguages.cs
│
├── 📁 Resources/
│   ├── AppIcon/
│   ├── Images/
│   ├── Languages/
│   │   ├── AppResources.resx
│   │   ├── AppResources.en.resx
│   │   └── AppResources.sv.resx
│   ├── Raw/
│   │   ├── tik_50ms_1000hz.wav
│   │   ├── tik_50ms_800hz.wav
│   │   └── ...
│   ├── Splash/
│   └── Styles/
│       ├── Colors.xaml
│       └── Styles.xaml
│
├── 📁 Services/
│   ├── Implementations/
│   │   ├── DeviceStatusService.cs
│   │   ├── LanguageService.cs
│   │   └── ThemeService.cs
│   └── Interfaces/
│       ├── IDeviceStatusService.cs
│       ├── ILanguageService.cs
│       └── IThemeService.cs
│
├── 📁 ViewModels/
│   ├── AboutViewModel.cs
│   ├── BaseViewModel.cs
│   ├── CompassViewModel.cs
│   ├── FlashlightViewModel.cs
│   ├── FramingViewModel.cs
│   ├── InstrumentBassPage.cs
│   ├── InstrumentCharangoPage.cs
│   ├── InstrumentNylonPage.cs
│   ├── InstrumentSteelPage.cs
│   ├── InstrumentUkulelePage.cs
│   ├── InstrumentViolinViewModel.cs
│   ├── ManualPage.cs
│   ├── MenuViewModel.cs
│   ├── MetronomeViewModel.cs
│   ├── ScreenLightViewModel.cs
│   └── TunerPage.cs
│
├── 📁 Views/
│   ├── AboutPage.xaml/cs
│   ├── CompassPage.xaml/cs
│   ├── FlashlightPage.xaml/cs
│   ├── FramingPage.xaml/cs
│   ├── InstrumentBassPage.xaml/cs
│   ├── InstrumentCharangoPage.xaml/cs
│   ├── InstrumentNylonPage.xaml/cs
│   ├── InstrumentSteelPage.xaml/cs
│   ├── InstrumentUkulelePage.xaml/cs
│   ├── InstrumentViolinViewModel.xaml/cs
│   ├── ManualPage.xaml/cs
│   ├── MenuPage.xaml/cs
│   ├── MetronomeViewModel.xaml/cs
│   ├── ScreenLightPage.xaml/cs
│   └── TunerViewModel.xaml/cs
│
├── App.xaml/cs
├── AppShell.xaml/cs
├── MauiProgram.cs
│
└── NavajaSuiza.Maui.csproj
```

## Release App
- Dhange debug to Release
- For apk : 
    - Right click project > Properties > Android Package Signing
    - Check "Sign the .APK using the following keystore details"
    - Fill in keystore details

## Languages 
- Resoures/Languages
    - AppResources.resx (Default)
    - AppResources.sv.resx
    - AppResources.en.resx
- AppShell.xaml
```xaml
xmlns:extensions="clr-namespace:NavajaSuiza_.NET10.Extensions"

Title="{extensions:Translate MenuText}"
```
- C#
```csharp
var selectLanguage = LocalizationResourceManager.Instance["SelectLanguageText"].ToString();
var cancelText = LocalizationResourceManager.Instance["CancelText"]?.ToString();
```
- Extensions
```csharp
public class LocalizationResourceManager : INotifyPropertyChanged
{
    private CultureInfo _culture;

    private LocalizationResourceManager()
    {
        Culture = CultureInfo.CurrentCulture;
    }

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    public static LocalizationResourceManager Instance { get; } = new();

    public object this[string resourceKey]
        => AppResources.ResourceManager.GetObject(resourceKey, _culture) ?? Array.Empty<byte>();

    public event PropertyChangedEventHandler PropertyChanged;
}
```
```csharp
[ContentProperty(nameof(Name))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Name { get; set; } 

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Name}]",
            Source = LocalizationResourceManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
```

