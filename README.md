# NavajaSuiza .NET10

Navaja suiza digital — app multiplataforma (.NET MAUI) con herramientas de uso diario: linterna, luz de pantalla, afinador de instrumentos, metrónomo, brújula, encuadre de imagen y más.

## Dependencias

| Paquete | Versión |
|---------|---------|
| CommunityToolkit.Maui | 15.0.1 |
| CommunityToolkit.Maui.MediaElement | 10.0.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Microsoft.Extensions.Logging.Debug | 10.0.11 |
| Microsoft.Maui.Controls | 10.0.100 |
| Syncfusion.Maui.Toolkit | 1.0.10 |

## Permisos Android (AndroidManifest)

- `BATTERY_STATUS`
- `CAMERA`
- `FLASHLIGHT`
- `ACCESS_COARSE_LOCATION`
- `ACCESS_FINE_LOCATION`

## Estructura

```
NavajaSuiza_.NET10.sln
│
├── NavajaSuiza.Core/                    # Class Library (net10.0)
│   ├── Interfaces/
│   │   ├── IDeviceStatusService.cs
│   │   ├── ILanguageService.cs
│   │   └── IThemeService.cs
│   ├── Models/
│   │   └── SupportedLanguages.cs
│   └── ViewModels/
│       └── BaseViewModel.cs
│
└── NavajaSuiza_.NET10/                  # Proyecto MAUI
    ├── Converters/
    │   ├── InvertedBoolConverter.cs
    │   └── StringNotEmptyToBoolConverter.cs
    ├── Extensions/
    │   ├── LocalizationResourceManager.cs
    │   └── TranslateExtension.cs
    ├── Models/
    │   └── InstrumentStringData.cs
    ├── Pages/
    │   ├── Components/
    │   │   ├── InstrumentBody.xaml/cs
    │   │   ├── InstrumentStringComponent.xaml/cs
    │   │   └── LoadingComponent.xaml/cs
    │   ├── AboutPage.xaml/cs
    │   ├── CompassPage.xaml/cs
    │   ├── FlashlightPage.xaml/cs
    │   ├── FramingPage.xaml/cs
    │   ├── InstrumentBassPage.xaml/cs
    │   ├── InstrumentCharangoPage.xaml/cs
    │   ├── InstrumentNylonPage.xaml/cs
    │   ├── InstrumentSteelPage.xaml/cs
    │   ├── InstrumentUkulelePage.xaml/cs
    │   ├── InstrumentViolinPage.xaml/cs
    │   ├── ManualPage.xaml/cs
    │   ├── MenuPage.xaml/cs
    │   ├── MetronomePage.xaml/cs
    │   ├── ScreenLightPage.xaml/cs
    │   ├── TestingPage.xaml/cs
    │   └── TunerPage.xaml/cs
    ├── Platforms/
    │   ├── Android/
    │   ├── iOS/
    │   ├── MacCatalyst/
    │   └── Windows/
    ├── Resources/
    │   ├── AppIcon/
    │   ├── Fonts/
    │   ├── Images/
    │   ├── Languages/
    │   │   ├── AppResources.resx       # Default (es-CL)
    │   │   ├── AppResources.en.resx    # Inglés
    │   │   └── AppResources.sv.resx    # Sueco
    │   ├── Raw/                        # Assets de audio WAV
    │   ├── Splash/
    │   └── Styles/
    │       ├── Colors.xaml
    │       └── Styles.xaml
    ├── Services/
    │   ├── Implementations/
    │   │   ├── DeviceStatusService.cs
    │   │   ├── InstrumentAudioService.cs
    │   │   ├── LanguageService.cs
    │   │   ├── MetronomeService.cs
    │   │   └── ThemeService.cs
    │   └── Interfaces/
    │       ├── IInstrumentAudioService.cs
    │       └── IMetronomeService.cs
    ├── ViewModels/
    │   ├── AboutViewModel.cs
    │   ├── BaseViewModel.cs
    │   ├── CompassViewModel.cs
    │   ├── FlashlightViewModel.cs
    │   ├── FramingViewModel.cs
    │   ├── InstrumentBassViewModel.cs
    │   ├── InstrumentCharangoViewModel.cs
    │   ├── InstrumentNylonViewModel.cs
    │   ├── InstrumentSteelViewModel.cs
    │   ├── InstrumentUkuleleViewModel.cs
    │   ├── InstrumentViewModelBase.cs
    │   ├── InstrumentViolinViewModel.cs
    │   ├── ManualViewModel.cs
    │   ├── MenuViewModel.cs
    │   ├── MetronomeViewModel.cs
    │   ├── ScreenLightViewModel.cs
    │   ├── TestingViewModel.cs
    │   └── TunerViewModel.cs
    ├── App.xaml/cs
    ├── AppShell.xaml/cs
    └── MauiProgram.cs
```

## Release App

### APK (Android)

1. Cambiar configuración de **Debug** a **Release**
2. Click derecho al proyecto `NavajaSuiza_.NET10` → **Properties**
3. Ir a **Android Signing**
4. Marcar **Sign the .APK using the following keystore details**
5. Completar los datos del keystore (alias, password, archivo `.keystore`)
6. Guardar
7. Build → Clean Solution
8. Build → Rebuild Solution
9. El `.apk` se genera en:
   ```
   bin/Release/net10.0-android/android-arm64/
   ```

### Windows

1. Cambiar a **Release**
2. Build → Clean Solution → Rebuild Solution
3. El ejecutable se genera en:
   ```
   bin/Release/net10.0-windows10.0.19041.0/win-x64/
   ```

## Languages

### Archivos de recursos
- `Resources/Languages/AppResources.resx` — Default (es-CL)
- `Resources/Languages/AppResources.en.resx` — Inglés
- `Resources/Languages/AppResources.sv.resx` — Sueco

### Uso en XAML
```xml
xmlns:extensions="clr-namespace:NavajaSuiza_.NET10.Extensions"

Title="{extensions:Translate MenuText}"
```

### Uso en C#
```csharp
var selectLanguage = LocalizationResourceManager.Instance["SelectLanguageText"].ToString();
var cancelText = LocalizationResourceManager.Instance["CancelText"]?.ToString();
```
