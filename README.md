# NavajaSuiza .NET10

## Dependencies
- CommunityToolkit.Maui
- CommunityToolkit.Mvvm
- Microsoft.Extensions.Logging.Debug
- Microsoft.Maui.Controls
- Syncfusion.Maui.Toolkit

## Structure
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
│   └── Languages/
│       ├── AppResources.resx
│       ├── AppResources.en.resx
│       └── AppResources.sv.resx
│
├── 📁 Services/
│   ├── Implementations/
│   │   └── LanguageService.cs
│   └── Interfaces/
│       └── ILanguageService.cs
│
├── 📁 ViewModels/
│   ├── BaseViewModel.cs
│   └── AboutViewModel.cs
│
├── 📁 Views/
│   ├── AboutPage.xaml/cs
│   ├── ManualPage.xaml/cs
│   └── AboutViewModel.xaml/cs
│
├── App.xaml/cs
├── AppShell.xaml/cs
├── MauiProgram.cs
│
└── NavajaSuiza.Maui.csproj
```

### Languages 
- App.xaml
```xaml
<Application

    <Application.Resources>
        <ResourceDictionary Source="Resources/Languages/es.xaml" />        
    </Application.Resources>
    
</Application>
```
