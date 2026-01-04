# NavajaSuiza .NET10

### Dependencies
- CommunityToolkit.Maui
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Logging.Debug
- Microsoft.Maui.Controls
- Syncfusion.Maui.Toolkit

### AndroidManifest permissions
- BATTERY_STATUS
- CAMERA
- FLASHLIGHT
- 

### Structure
```
NavajaSuiza_.NET10/
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
│   ├── Splash/
│   └── Styles/
│       ├── Colors.xaml
│       └── Styles.xaml
│
├── 📁 Services/
│   ├── Implementations/
│   │   └── LanguageService.cs
│   └── Interfaces/
│       └── ILanguageService.cs
│
├── 📁 ViewModels/
│   ├── AboutViewModel.cs
│   ├── BaseViewModel.cs
│   ├── FlashlightViewModel.cs
│   ├── MenuViewModel.cs
│   └── ScreenLightViewModel.cs
│
├── 📁 Views/
│   ├── AboutPage.xaml/cs
│   ├── FlashlightPage.xaml/cs
│   ├── ManualPage.xaml/cs
│   ├── MenuPage.xaml/cs
│   └── ScreenLightPage.xaml/cs
│
├── App.xaml/cs
├── AppShell.xaml/cs
├── MauiProgram.cs
│
└── NavajaSuiza.Maui.csproj
```

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

