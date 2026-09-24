# NavajaSuiza .NET10

Navaja suiza digital — app multiplataforma (.NET MAUI) con herramientas de uso diario: linterna, luz de pantalla, afinador de instrumentos, metrónomo, brújula, encuadre de imagen y más.

## Dependencias

| Paquete | Versión |
|---------|---------|
| CommunityToolkit.Maui | 15.0.1 |
| CommunityToolkit.Maui.MediaElement | 10.0.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| Microsoft.Extensions.Logging.Debug | 10.0.12 |
| Microsoft.Maui.Controls | 10.0.110 |
| SkiaSharp | 4.152.1 |
| sqlite-net-pcl | 1.11.285 |
| Syncfusion.Maui.Toolkit | 1.0.11 |
| Syncfusion.Maui.PdfViewer | 34.2.9 |
| Syncfusion.DocIORenderer.NET | 34.2.9 |
| Syncfusion.XlsIORenderer.NET | 34.2.9 |
| Syncfusion.Licensing | 34.2.9 |
| Markdig | 1.4.0 |

## Permisos Android (AndroidManifest)

- `CAMERA`
- `FLASHLIGHT`

`INTERNET` lo re-inyecta `CommunityToolkit.Maui` en el manifest fusionado (permiso *normal*, se conserva). Eliminados por principio de mínimo privilegio: `BATTERY_STATS`, `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`, `ACCESS_NETWORK_STATE`.

## Estructura

```
NavajaSuiza_.NET10.sln
│
├── NavajaSuiza.Core/                    # Class Library (net10.0 puro, sin dependencias MAUI)
│   ├── Interfaces/                      # 23 interfaces
│   │   ├── IAppInfoService.cs
│   │   ├── ICompassService.cs
│   │   ├── IDeviceDisplayService.cs
│   │   ├── IDeviceStatusService.cs
│   │   ├── IDocumentPdfConverter.cs
│   │   ├── IFilePickerService.cs
│   │   ├── IFlashlightService.cs
│   │   ├── IFlashlightStateService.cs
│   │   ├── IImagePickerService.cs
│   │   ├── IInstrumentAudioService.cs
│   │   ├── ILanguageService.cs
│   │   ├── ILauncherService.cs
│   │   ├── IMarkdownToHtmlConverter.cs
│   │   ├── IMetronomeService.cs
│   │   ├── IMorseSignalService.cs
│   │   ├── INavigationService.cs
│   │   ├── INotesRepository.cs
│   │   ├── IOrientationService.cs
│   │   ├── IPizarraImageExporter.cs
│   │   ├── IScreenBrightnessService.cs
│   │   ├── IStopwatchService.cs
│   │   ├── IThemeService.cs
│   │   └── ITimeSource.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   ├── InstrumentStringData.cs
│   │   ├── PizarraStroke.cs
│   │   ├── StopwatchLap.cs
│   │   ├── DocumentType.cs
│   │   ├── PdfReaderPayload.cs
│   │   ├── MorseSignalSequence.cs
│   │   └── Note.cs
│   ├── Services/
│   │   ├── FlashlightStateService.cs
│   │   ├── StopwatchService.cs
│   │   ├── MarkdownToHtmlConverter.cs
│   │   ├── MorseSignalService.cs
│   │   └── DocumentTypeDetector.cs
│   ├── ViewModels/                       # 24 archivos (testables, sin dependencias MAUI)
│   │   ├── BaseViewModel.cs
│   │   ├── AboutViewModel.cs
│   │   ├── CompassViewModel.cs
│   │   ├── FlashlightViewModel.cs
│   │   ├── FramingViewModel.cs
│   │   ├── GuideViewModel.cs
│   │   ├── MenuViewModel.cs
│   │   ├── MetronomeViewModel.cs
│   │   ├── ManualViewModel.cs
│   │   ├── NoteEditorViewModel.cs
│   │   ├── NotesViewModel.cs
│   │   ├── PdfReaderViewModel.cs
│   │   ├── TextReaderViewModel.cs
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
│   │   └── Implementations/             # 19 implementaciones (APIs de plataforma + repos de datos)
│   │       ├── AppInfoService.cs
│   │       ├── CompassSensorService.cs
│   │       ├── DeviceDisplayService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── DocumentPdfConverter.cs
│   │       ├── FilePickerService.cs
│   │       ├── FlashlightService.cs
│   │       ├── ImagePickerService.cs
│   │       ├── InstrumentAudioService.cs
│   │       ├── LanguageService.cs
│   │       ├── LauncherService.cs
│   │       ├── MetronomeService.cs
│   │       ├── NavigationService.cs
│   │       ├── NoteEntity.cs
│   │       ├── OrientationSensorService.cs
│   │       ├── PizarraImageExporter.cs
│   │       ├── RealtimeTimeSource.cs
│   │       ├── ScreenBrightnessService.cs
│   │       ├── SqliteNotesRepository.cs
│   │       └── ThemeService.cs
│   ├── Converters/
│   ├── Extensions/
│   ├── Resources/
│   │   ├── Languages/
│   │   │   ├── AppResources.resx       # Default (es-CL)
│   │   │   ├── AppResources.en.resx    # Inglés
│   │   │   └── AppResources.sv.resx    # Sueco
│   │   ├── Raw/
│   │   │   ├── Audio/                   # Assets de audio WAV (37 archivos)
│   │   │   └── guide/                   # Guía del usuario (USER_GUIDE.{es,en,sv}.md + imágenes)
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
│   ├── FlashlightStateServiceTests.cs
│   ├── FlashlightViewModelTests.cs
│   ├── FramingViewModelTests.cs
│   ├── InstrumentStringDataTests.cs
│   ├── InstrumentViewModelTests.cs
│   ├── MarkdownToHtmlConverterTests.cs
│   ├── MenuViewModelTests.cs
│   ├── MetronomeViewModelTests.cs
│   ├── MorseSignalSequenceTests.cs
│   ├── MorseSignalServiceTests.cs
│   ├── NoteEditorViewModelTests.cs
│   ├── NotesViewModelTests.cs
│   ├── PdfReaderViewModelTests.cs
│   ├── PizarraViewModelTests.cs
│   ├── ScreenLightViewModelTests.cs
│   ├── StopwatchServiceTests.cs
│   ├── StopwatchViewModelTests.cs
│   └── TunerViewModelTests.cs
│
├── .editorconfig                        # Convenciones de código
└── .github/workflows/build.yml          # CI/CD: build + test en push/PR
```

## Release App

> **Antes de publicar**: registra tu clave de licencia Syncfusion (**Community License**, gratuita y definitiva; la *trial* dura 30 días y genera aviso en el visor de PDF — confirma el tipo en tu panel de cuentas de Syncfusion antes de publicar). La clave se configura **una sola vez** como variable de entorno de Windows: Inicio → "Variables de entorno" → **Nueva** en *Variables de usuario* → nombre `SYNC_FUSION_LICENSE_KEY`, valor la clave → reiniciar Visual Studio. En build se inyecta como `AssemblyMetadata` desde esa variable o con `-p:SyncfusionLicenseKey=...`; **nunca se versiona en el repositorio**. Sin clave configurada, el proyecto compila igual (sin registro).

### AAB (Google Play)

Google Play **no acepta APK**, requiere un **App Bundle (.aab)** firmado. El `csproj` genera `.aab` en Release Android por defecto.

1. Cambiar configuración de **Debug** a **Release**
2. Click derecho al proyecto `NavajaSuiza_.NET10` → **Archive**
3. Seleccionar **Signing** (usar el keystore de producción, guardarlo de forma permanente y con backup: perderlo impide actualizar la app)
4. Subir el `.aab` generado a **Play Console**. Play App Signing firma el APK final; el keystore es la *upload key*

> El keystore (`.keystore`, `.jks`, `.p12`) está excluido del repo vía `.gitignore`. Guárdalo **fuera** del repositorio y respáldalo de forma segura.

> **Regla**: el keystore del APK de prueba debe ser el **mismo** que el del AAB final; si cambias de firma, Play rechaza la actualización.

> **Dónde NO configurar la firma**: en *Properties → Android Signing* (Android Firma de Paquete) las contraseñas quedan en el `.csproj`, que **se versiona en GitHub**. Usar el flujo **Archive → Ad Hoc** (perfil de firma fuera del proyecto) o las properties por línea de comandos/CI.

> **El diálogo Ad Hoc no lleva la clave Syncfusion**: ahí solo van los datos del keystore (alias, contraseña, archivo). La licencia va en la variable `SYNC_FUSION_LICENSE_KEY` (ver arriba).

> Para generar un **APK** de prueba (instalación directa en dispositivo), compilar Release con `-p:AndroidPackageFormat=apk`. El APK Release es **multirarquitectura** (fat APK): incluye `arm64-v8a`, `armeabi-v7a` (32-bit) y `x86_64` gracias a los `RuntimeIdentifiers` del `.csproj`.

> Si no tienes keystore, créalo con `keytool -genkey -v -keystore filename.keystore -alias alias -keyalg RSA -keysize 2048 -validity 10000` (requiere un JDK en la máquina), o desde el wizard de **Archive** de Visual Studio (Tools → Android → Archive → "+" en Signing).

### APK (Android)

> Probar en dispositivo real con **Archive → Distribuir → Ad Hoc** (selecciona el keystore en el diálogo). Evitar *Properties → Android Signing*: escribe contraseñas en el `.csproj` versionado.

1. Cambiar configuración de **Debug** a **Release**
2. Click derecho al proyecto `NavajaSuiza_.NET10` → **Archive**
3. Esperar la compilación → **Distribuir → Ad Hoc**
4. Seleccionar/crear el keystore (mismo que se usará para el AAB final)
5. Instalar el APK en el dispositivo, o compilar por CLI:
   - Estado del arte: con `-p:AndroidPackageFormat=apk`, el Release genera el APK firmado en:
   ```
   bin/Release/net10.0-android/com.nefelin.navajasuiza-Signed.apk
   ```
   - Sin ese parámetro (default), el Release genera el AAB en:
   ```
   bin/Release/net10.0-android/com.nefelin.navajasuiza.aab
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

## Licencia

Este proyecto se distribuye bajo la **Licencia MIT** (ver `LICENSE.txt`).

> **Aviso sobre componentes de terceros:** este proyecto utiliza componentes de [Syncfusion](https://www.syncfusion.com) (`Syncfusion.DocIORenderer.NET`, `Syncfusion.XlsIORenderer.NET`, `Syncfusion.Maui.PdfViewer`, `Syncfusion.Maui.Toolkit`), que **no están cubiertos por la licencia MIT** de este repositorio. Los componentes Syncfusion se usan bajo su licencia correspondiente y siguen sujetos a los términos y condiciones de Syncfusion. No redistribuyas los binarios de Syncfusion fuera de los términos permitidos por su licencia.
