# NavajaSuiza .NET10

Navaja suiza digital — app multiplataforma (.NET MAUI) con herramientas de uso diario: linterna, luz de pantalla, afinador de instrumentos, metrónomo, brújula, encuadre de imagen y más.

## Dependencias

| Paquete | Versión |
|---------|---------|
| CommunityToolkit.Maui | 15.0.1 |
| CommunityToolkit.Maui.MediaElement | 10.0.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Microsoft.Extensions.Logging.Debug | 10.0.12 |
| Microsoft.Maui.Controls | 10.0.101 |
| SkiaSharp | 4.152.1 |
| sqlite-net-pcl | 1.11.285 |
| Syncfusion.Maui.Toolkit | 1.0.11 |

## Permisos Android (AndroidManifest)

- `CAMERA`
- `FLASHLIGHT`

`INTERNET` lo re-inyecta `CommunityToolkit.Maui` en el manifest fusionado (permiso *normal*, se conserva). Eliminados por principio de mínimo privilegio: `BATTERY_STATS`, `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`, `ACCESS_NETWORK_STATE`.

## Estructura

```
NavajaSuiza_.NET10.sln
│
├── NavajaSuiza.Core/                    # Class Library (net10.0 puro, sin dependencias MAUI)
│   ├── Interfaces/                      # 16 interfaces
│   │   ├── ICompassService.cs
│   │   ├── IDeviceDisplayService.cs
│   │   ├── IDeviceStatusService.cs
│   │   ├── IFilePickerService.cs
│   │   ├── IFlashlightService.cs
│   │   ├── IFlashlightStateService.cs
│   │   ├── IImagePickerService.cs
│   │   ├── IInstrumentAudioService.cs
│   │   ├── ILanguageService.cs
│   │   ├── IMetronomeService.cs
│   │   ├── INavigationService.cs
│   │   ├── IOrientationService.cs
│   │   ├── IPizarraImageExporter.cs
│   │   ├── IScreenBrightnessService.cs
│   │   ├── IStopwatchService.cs
│   │   └── IThemeService.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   ├── InstrumentStringData.cs
│   │   ├── PizarraStroke.cs
│   │   └── StopwatchLap.cs
│   ├── Services/
│   │   ├── FlashlightStateService.cs
│   │   └── StopwatchService.cs
│   ├── ViewModels/                       # 21 archivos (testables, sin dependencias MAUI)
│   │   ├── BaseViewModel.cs
│   │   ├── AboutViewModel.cs
│   │   ├── CompassViewModel.cs
│   │   ├── FlashlightViewModel.cs
│   │   ├── FramingViewModel.cs
│   │   ├── MenuViewModel.cs
│   │   ├── MetronomeViewModel.cs
│   │   ├── ManualViewModel.cs
│   │   ├── PdfReaderViewModel.cs
│   │   ├── PizarraViewModel.cs
│   │   ├── ScreenLightViewModel.cs
│   │   ├── StopwatchViewModel.cs
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
│   │   └── Implementations/             # 14 implementaciones (solo APIs de plataforma)
│   │       ├── CompassSensorService.cs
│   │       ├── DeviceDisplayService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── FilePickerService.cs
│   │       ├── FlashlightService.cs
│   │       ├── ImagePickerService.cs
│   │       ├── InstrumentAudioService.cs
│   │       ├── LanguageService.cs
│   │       ├── MetronomeService.cs
│   │       ├── NavigationService.cs
│   │       ├── OrientationSensorService.cs
│   │       ├── PizarraImageExporter.cs
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
│   ├── DocumentTypeDetectorTests.cs
│   ├── FlashlightViewModelTests.cs
│   ├── FramingViewModelTests.cs
│   ├── InstrumentStringDataTests.cs
│   ├── MenuViewModelTests.cs
│   ├── MetronomeViewModelTests.cs
│   ├── PdfReaderViewModelTests.cs
│   ├── PizarraViewModelTests.cs
│   ├── ScreenLightViewModelTests.cs
│   ├── StopwatchServiceTests.cs
│   ├── StopwatchViewModelTests.cs
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
   bin/Release/net10.0-android/com.nefelin.navajasuiza-Signed.apk
   ```

> El APK Release es **multirarquitectura** (fat APK): incluye `arm64-v8a`, `armeabi-v7a` (32-bit) y `x86_64` gracias a los `RuntimeIdentifiers` del `.csproj` (que reemplazan a la obsoleta `AndroidSupportedAbis`). Los subdirectorios por RID (`android-arm64/`, etc.) solo se generan al publicar un RID único.

> Si no tenés un keystore, crearlo con `keytool -genkey -v -keystore filename.keystore -alias alias -keyalg RSA -keysize 2048 -validity 10000` (requiere un JDK en la máquina), o desde el wizard de **Archive** de Visual Studio (Tools → Android → Archive → "+" en Signing), que usa el JDK con el que VS ya compila Android.

### AAB (Google Play)

Google Play no acepta APK directamente: requiere un **App Bundle (.aab)** firmado. Vía Visual Studio: click derecho al proyecto → **Archive** → seleccionar **Signing** (usar el keystore de producción, guardarlo de forma permanente) → generará el `.aab` que se sube a Play Console. Play App Signing firma el APK final; tu keystore es la *upload key*.

- Otra forma de hacerlo
1. Cambiar configuración de **Debug** a **Release**
2. Click derecho al proyecto `NavajaSuiza_.NET10` → **Properties**
3. Id a Android → Options y seleccionar en Formato de paquete Android .apk
4. Click derecho al proyecto `NavajaSuiza_.NET10` → **Publish**
5. Cuando termine ir a Distribute... y hacer click en Ad Hoc
6. Click en el + y crear la KEY:         
    - Alias: Key
    - Password: ******
    - Validity: 30
    - Name: MyName
    - Organizational Unit: Dev
    - Organization: MyCompany
    - City or Location: Valparaíso
    - State or Province: Valparaíso
    - Country Code: CL
7. Seleccionar la llave y Save As
8. Guardar la llave
9. Ingresar contraseña, y al terminar de compilar hacer click en Open distribution
 
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
