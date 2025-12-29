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
├── 📁 Models/
│   ├── ApiResponse.cs
│   ├── CoreSecretData.cs
│   ├── CoreUserIV.cs
│   ├── SessionData.cs
│   └── User.cs
│
├── 📁 Resources/
│   └── Languages/
│       ├── es.xaml
│       ├── en.xaml
│       └── sv.xaml
│
├── 📁 Services/
│   ├── Implementations/
│   │   ├── LanguageService.cs
│   │   ├── SecureStorageService.cs
│   │   └── ThemeService.cs
│   └── Interfaces/
│       ├── ILanguageService.cs
│       ├── ISecureStorageService.cs
│       └── IThemeService.cs
│
├── 📁 ViewModels/
│   ├── PasswordFormViewModel.cs
│   ├── SettingsViewModel.cs
│   └── TestingViewModel.cs
│
├── 📁 Views/
│   ├── SettingsPage.xaml
│   ├── SettingsPage.xaml
│   └── SettingsPage.xaml
│
├── App.xaml
├── App.xaml.cs
├── AppShell.xaml
├── AppShell.xaml.cs
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
