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
├── NavajaSuiza.Core/                    # Class Library (net10.0 puro, sin dependencias MAUI)
│   ├── Interfaces/                      # 13 interfaces
│   │   ├── ICompassService.cs
│   │   ├── IDeviceDisplayService.cs
│   │   ├── IDeviceStatusService.cs
│   │   ├── IFlashlightService.cs
│   │   ├── IFlashlightStateService.cs
│   │   ├── IImagePickerService.cs
│   │   ├── IInstrumentAudioService.cs
│   │   ├── ILanguageService.cs
│   │   ├── IMetronomeService.cs
│   │   ├── INavigationService.cs
│   │   ├── IOrientationService.cs
│   │   ├── IScreenBrightnessService.cs
│   │   └── IThemeService.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   └── InstrumentStringData.cs
│   ├── Services/
│   │   └── FlashlightStateService.cs
│   ├── ViewModels/                       # 18 ViewModels (testables, sin dependencias MAUI)
│   │   ├── BaseViewModel.cs
│   │   ├── AboutViewModel.cs
│   │   ├── CompassViewModel.cs
│   │   ├── FlashlightViewModel.cs
│   │   ├── FramingViewModel.cs
│   │   ├── MenuViewModel.cs
│   │   ├── MetronomeViewModel.cs
│   │   ├── ManualViewModel.cs
│   │   ├── ScreenLightViewModel.cs
│   │   ├── TestingViewModel.cs
│   │   ├── TunerViewModel.cs
│   │   ├── InstrumentViewModelBase.cs
│   │   ├── InstrumentBassViewModel.cs
│   │   ├── InstrumentCharangoViewModel.cs
│   │   ├── InstrumentNylonViewModel.cs
│   │   ├── InstrumentSteelViewModel.cs
│   │   ├── InstrumentUkuleleViewModel.cs
│   │   └── InstrumentViolinViewModel.cs
│   └── AppConstants.cs
│
├── NavajaSuiza_.NET10/                  # Proyecto MAUI
│   ├── Pages/
│   │   ├── Components/
│   │   │   ├── InstrumentBody.xaml/cs
│   │   │   ├── InstrumentStringComponent.xaml/cs
│   │   │   └── LoadingComponent.xaml/cs
│   │   └── *.xaml/cs
│   ├── ViewModels/                       # Vacío — todas las VMs están en Core
│   ├── Services/
│   │   └── Implementations/             # 12 implementaciones (solo APIs de plataforma)
│   │       ├── CompassSensorService.cs
│   │       ├── DeviceDisplayService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── FlashlightService.cs
│   │       ├── ImagePickerService.cs
│   │       ├── InstrumentAudioService.cs
│   │       ├── LanguageService.cs
│   │       ├── MetronomeService.cs
│   │       ├── NavigationService.cs
│   │       ├── OrientationSensorService.cs
│   │       ├── ScreenBrightnessService.cs
│   │       └── ThemeService.cs
│   ├── Converters/
│   ├── Extensions/
│   ├── Resources/
│   │   ├── Languages/
│   │   │   ├── AppResources.resx       # Default (es-CL)
│   │   │   ├── AppResources.en.resx    # Inglés
│   │   │   └── AppResources.sv.resx    # Sueco
│   │   ├── Raw/                        # Assets de audio WAV (37 archivos)
│   │   └── Styles/
│   ├── Platforms/
│   ├── App.xaml/cs
│   ├── AppShell.xaml/cs
│   └── MauiProgram.cs
│
├── NavajaSuiza.Test/                    # Proyecto de tests (net10.0 puro)
│   ├── AboutViewModelTests.cs
│   ├── AppConstantsTests.cs
│   ├── BaseViewModelTests.cs
│   ├── CompassViewModelTests.cs
│   ├── FlashlightViewModelTests.cs
│   ├── FramingViewModelTests.cs
│   ├── InstrumentStringDataTests.cs
│   ├── MenuViewModelTests.cs
│   ├── MetronomeViewModelTests.cs
│   └── ScreenLightViewModelTests.cs
│
├── .editorconfig                        # Convenciones de código
└── .github/workflows/build.yml          # CI/CD: build + test en push/PR
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

## CI/CD

GitHub Actions workflow en `.github/workflows/build.yml`:
- Ejecuta en push y PR a `main`
- Steps: `dotnet restore` → `dotnet build` → `dotnet test`

## Convenciones de código

`.editorconfig` en raíz del repo con reglas de naming, formato y suppressions de analyzers.
