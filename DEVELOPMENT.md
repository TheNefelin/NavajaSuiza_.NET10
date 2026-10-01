# DEVELOPMENT.md - NavajaSuiza .NET10

Contexto técnico, arquitectura y evolución del proyecto.

---

## 1. Descripción general

**NavajaSuiza** es una aplicación multiplataforma construida con **.NET MAUI** (net10.0) que funciona como un conjunto de herramientas de uso diario. El concepto es una "navaja suiza digital": múltiples utilidades compactas en una sola app, con énfasis en herramientas musicales. El proyecto seguirá escalando con nuevas opciones.

**Plataformas objetivo**: Android, iOS, macOS (Catalyst), Windows.

---

## 2. Stack tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Framework | .NET MAUI | net10.0 |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.4.2 |
| UI Toolkit | CommunityToolkit.Maui | 15.0.1 |
| Media | CommunityToolkit.Maui.MediaElement | 10.0.0 |
| Markdown | Markdig | 1.4.0 |
| PDF (visor) | Syncfusion.Maui.PdfViewer | 34.2.9 |
| DocIO (conversión DOC/DOCX) | Syncfusion.DocIORenderer.NET | 34.2.9 |
| XlsIO (conversión XLS/XLSX/CSV) | Syncfusion.XlsIORenderer.NET | 34.2.9 |
| Licencia Syncfusion | Syncfusion.Licensing | 34.2.9 |
| UI Components (gratuito) | Syncfusion.Maui.Toolkit | 1.0.11 |
| Logging | Microsoft.Extensions.Logging.Debug | 10.0.12 |
| Controls | Microsoft.Maui.Controls | 10.0.110 |

---

## 3. Arquitectura

### 3.1 Decisión arquitectónica: Core + MAUI

**Fecha de decisión**: 2026-08-25  
**Estado**: Implementada

#### Problema

El proyecto inicial era un monolito MAUI donde todo vivía en un solo `.csproj`: UI, ViewModels, servicios, modelos, extensions y resources. Para una app con 11 herramientas que **seguirá escalando**, esto presenta un problema de reutilización: si mañana se necesita una app web, otra MAUI o un servicio que reutilice la lógica de instrumentos, metrónomo o procesamiento de imagen, habría que copiar código.

#### Opciones evaluadas

| Opción | Descripción | Ventaja | Desventaja | Veredicto |
|--------|------------|---------|------------|-----------|
| **A: Monolito MAUI** | Todo en un solo proyecto | Simple | No escala, no reutiliza | Descartada |
| **B: Core + MAUI** | Class Library + proyecto MAUI | Reutilizable, equilibrado | Un nivel más de abstracción | **Seleccionada** |
| **C: Clean Architecture** | Domain + Application + Infrastructure + UI | Extremadamente mantenible | Over-engineering para herramientas independientes | Descartada |

#### Justificación de la Opción B

1. **Reutilización**: La lógica de negocio (servicios, modelos) vive en una `Class Library` que puede ser referenciada desde cualquier proyecto .NET — MAUI, consola, web, tests.
2. **Separación de responsabilidades**: Lo que depende de APIs de plataforma (sensores, flash, brillo) se queda en MAUI. Lo que es pura lógica va a Core.
3. **Escalabilidad**: Agregar nuevas herramientas no implica duplicar infraestructura.
4. **Simplicidad**: No se justifica Clean Architecture completa porque cada herramienta es independiente — no hay lógica de negocio cruzada compleja entre herramientas.

#### ¿Por qué NO Clean Architecture?

Clean Architecture resuelve problemas de **dominio complejo** (reglas de negocio cruzadas, múltiples fuentes de datos, validaciones compuestas). Una navaja suiza tiene herramientas independientes que no comparten reglas de negocio. Aplicar Clean Architecture sería over-engineering que agrega complejidad sin beneficio tangible.

### 3.2 Patrón: MVVM

La aplicación sigue el patrón **MVVM** (Model-View-ViewModel) utilizing el CommunityToolkit.Mvvm:

- **Models**: Modelos de datos simples (`InstrumentStringData`, `SupportedLanguages`).
- **Views** (Pages): Páginas XAML con code-behind mínimo (solo inicialización de BindingContext y ciclos de vida).
- **ViewModels**: Lógica de presentación, comandos y estado de UI. Extienden `BaseViewModel`.
- **Services**: Lógica de negocio y acceso a APIs de plataforma, separada de los ViewModels mediante interfaces.

**Requisito de build**: `<LangVersion>preview</LangVersion>` en ambos csproj (Core y MAUI). CommunityToolkit.Mvvm 8.4.2 genera `INotifyPropertyChanged` correctamente para `partial properties` solo con C# preview (keyword `field`). Sin esta bandera, el source generator cae al fallback field-based y los Bindings no reciben notificaciones de cambio.

### 3.3 Estructura actual

```
NavajaSuiza_.NET10/                   # Solution
├── NavajaSuiza.Core/                 # Class Library (net10.0 puro, sin dependencias MAUI)
│   ├── Interfaces/                   # 23 interfaces
│   │   ├── ILanguageService.cs
│   │   ├── IThemeService.cs
│   │   ├── IDeviceStatusService.cs
│   │   ├── INavigationService.cs
│   │   ├── IInstrumentAudioService.cs
│   │   ├── IMetronomeService.cs
│   │   ├── ICompassService.cs
│   │   ├── IOrientationService.cs
│   │   ├── ICompassPositionService.cs
│   │   ├── IFlashlightService.cs
│   │   ├── IDeviceDisplayService.cs
│   │   ├── IImagePickerService.cs
│   │   ├── IScreenBrightnessService.cs
│   │   ├── IFlashlightStateService.cs
│   │   ├── IStopwatchService.cs
│   │   ├── IFilePickerService.cs
│   │   ├── IDocumentPdfConverter.cs
│   │   ├── IAppInfoService.cs
│   │   ├── ILauncherService.cs
│   │   ├── IMarkdownToHtmlConverter.cs
│   │   ├── INotesRepository.cs
│   │   ├── IMorseSignalService.cs
│   │   ├── IPizarraImageExporter.cs
│   │   └── ITimeSource.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   ├── InstrumentStringData.cs
│   │   ├── StopwatchLap.cs
│   │   ├── PizarraStroke.cs
│   │   ├── DocumentType.cs
│   │   ├── PdfReaderPayload.cs
│   │   ├── MorseSignalSequence.cs
│   │   ├── PositionReading.cs
│   │   └── Note.cs
│   ├── Services/
│   │   ├── FlashlightStateService.cs   # Singleton: persiste estado flash entre VM recreations
│   │   ├── StopwatchService.cs         # Singleton: cronómetro (Stopwatch + PeriodicTimer), thread-safe
│   │   ├── MarkdownToHtmlConverter.cs  # Singleton: Markdig → HTML (assets embebidos como data URI)
│   │   ├── MorseSignalService.cs       # Generación/parsing de secuencias Morse
│   │   └── DocumentTypeDetector.cs     # Detección de tipo real por contenido (PDF/DOCX/XLSX/CSV/Texto)
│   ├── AppConstants.cs
│   └── ViewModels/                     # 24 ViewModels (todas testables, sin dependencias MAUI)
│       ├── BaseViewModel.cs
│       ├── AboutViewModel.cs
│       ├── MenuViewModel.cs
│       ├── MetronomeViewModel.cs
│       ├── StopwatchViewModel.cs
│       ├── TunerViewModel.cs
│       ├── ManualViewModel.cs
│       ├── CompassViewModel.cs
│       ├── FlashlightViewModel.cs
│       ├── FramingViewModel.cs
│       ├── ScreenLightViewModel.cs
│       ├── InstrumentViewModelBase.cs
│       ├── InstrumentBassViewModel.cs
│       ├── InstrumentCharangoViewModel.cs
│       ├── InstrumentNylonViewModel.cs
│       ├── InstrumentSteelViewModel.cs
│       ├── InstrumentUkuleleViewModel.cs
│       ├── InstrumentViolinViewModel.cs
│       ├── GuideViewModel.cs
│       ├── PizarraViewModel.cs
│       ├── PdfReaderViewModel.cs
│       ├── TextReaderViewModel.cs
│       ├── NotesViewModel.cs
│       └── NoteEditorViewModel.cs
│
├── NavajaSuiza_.NET10/               # Proyecto MAUI
│   ├── Pages/
│   │   ├── Components/
│   │   │   ├── InstrumentBody.xaml
│   │   │   ├── InstrumentStringComponent.xaml
│   │   │   └── LoadingComponent.xaml
│   │   └── *.xaml / *.xaml.cs
│   ├── ViewModels/                     # Vacío — todas las VMs están en Core
│   ├── Services/
│   │   └── Implementations/           # 20 implementaciones (las que usan APIs de plataforma + repos de datos)
│   │       ├── LanguageService.cs
│   │       ├── ThemeService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── NavigationService.cs
│   │       ├── InstrumentAudioService.cs
│   │       ├── MetronomeService.cs
│   │       ├── CompassSensorService.cs
│   │       ├── OrientationSensorService.cs
│   │       ├── CompassPositionService.cs  # ICompassPositionService (Geolocation bajo demanda)
│   │       ├── FlashlightService.cs
│   │       ├── DeviceDisplayService.cs
│   │       ├── ImagePickerService.cs
│   │       ├── ScreenBrightnessService.cs
│   │       ├── FilePickerService.cs
│   │       ├── DocumentPdfConverter.cs
│   │       ├── LauncherService.cs
│   │       ├── AppInfoService.cs
│   │       ├── SqliteNotesRepository.cs     # INotesRepository (SQLite local)
│   │       ├── RealtimeTimeSource.cs        # ITimeSource
│   │       └── PizarraImageExporter.cs      # IPizarraImageExporter (SkiaSharp)
│   ├── Converters/
│   ├── Extensions/
│   ├── Resources/
│   │   ├── Languages/
│   │   ├── Raw/                     # Assets de audio WAV
│   │   └── Styles/
│   ├── Platforms/
│   ├── App.xaml/cs
│   ├── AppShell.xaml/cs
│   └── MauiProgram.cs
│
└── NavajaSuiza.Test/                # Proyecto de tests (net10.0 puro)
    └── *Tests.cs                     # xUnit + Moq, 230 tests
```

#### Regla de separación Core vs MAUI

| Va a Core (reutilizable, net10.0 puro) | Se queda en MAUI (depende de APIs de plataforma) |
|----------------------------------------|--------------------------------------------------|
| `BaseViewModel` | `NavigationService` (usa `Shell.Current`) |
| Todos los ViewModels (24) | `LanguageService` (usa `Preferences`, `CultureInfo`) |
| `ICompassService`, `IOrientationService` | `ThemeService` (usa `Application.Current`, Android Window) |
| `IFlashlightService`, `IFlashlightStateService` | `DeviceStatusService` (usa `Battery.Default`, Android APIs) |
| `IDeviceDisplayService`, `IImagePickerService` | `CompassSensorService` (usa `Compass.Default`, `OrientationSensor`) |
| `ICompassPositionService` | `CompassPositionService` (usa `Geolocation.Default`, `Permissions`) |
| `IScreenBrightnessService` | `OrientationSensorService` (usa `OrientationSensor.Default`) |
| `INavigationService`, `ILanguageService`, `IThemeService` | `FlashlightService` (usa `Flashlight.Default`) |
| `IInstrumentAudioService`, `IMetronomeService` | `DeviceDisplayService` (usa `DeviceDisplay.Current`) |
| `InstrumentStringData`, `SupportedLanguages` | `ImagePickerService` (usa `FilePicker`) |
| `AppConstants` | `ScreenBrightnessService` (usa Android brightness APIs) |
| `FlashlightStateService` (Core.Services) | `InstrumentAudioService` (usa `MediaElement`, `Border`) |
| `StopwatchService` (Core.Services) | `MetronomeService` (usa `MediaElement`) |

**Patrón para desacoplar MAUI types en interfaces Core**: Las interfaces `IInstrumentAudioService` e `IMetronomeService` usan `object` en lugar de `MediaElement`/`Border` para no depender de MAUI. Las implementaciones en MAUI hacen el cast explícito.

---

## 4. Navegación

### 4.1 Shell y TabBar

La navegación se basa en **MAUI Shell** con un `TabBar` de dos pestañas:

- **Menú** (`MenuPage`): Página principal con acceso a todas las herramientas.
- **Acerca de** (`AboutPage`): Configuración de tema y créditos.

### 4.2 Navegación entre páginas

Las herramientas se acceden desde `MenuPage` mediante comandos que resuelven páginas desde DI y navegan con `Shell.Current.Navigation.PushAsync()`. Las páginas de instrumentos se acceden desde `TunerPage`.

```
MenuPage
├── FlashlightPage
│   └── ScreenLightPage
├── FramingPage
├── TunerPage
│   ├── InstrumentNylonPage
│   ├── InstrumentSteelPage
│   ├── InstrumentBassPage
│   ├── InstrumentUkulelePage
│   ├── InstrumentViolinPage
│   └── InstrumentCharangoPage
├── MetronomePage
├── StopwatchPage
├── CompassPage
├── ManualPage
└── AboutPage (también en TabBar)
```

### 4.3 ToolbarItem (cambio de idioma)

Un `ToolbarItem` en el `AppShell` permite cambiar entre español, inglés y sueco mediante un `DisplayActionSheet`.

---

## 5. Servicios

### 5.1 Registro actual

Todos los servicios están registrados en `MauiProgram.cs` e inyectados por DI.

| Interfaz (Core) | Implementación (MAUI) | Lifetime | Responsabilidad |
|-----------------|----------------------|----------|-----------------|
| `ILanguageService` | `LanguageService` | Singleton | Localización, cambio de idioma, persistencia en `Preferences` |
| `IThemeService` | `ThemeService` | Singleton | Tema oscuro/claro, persistencia, status bar (Android) |
| `IDeviceStatusService` | `DeviceStatusService` | Singleton | Batería y almacenamiento en **valores numéricos** (`GetBatteryCapacity`, `GetAvailableStorageBytes`, `GetTotalStorageBytes`); devuelve `-1` en fallo |
| `INavigationService` | `NavigationService` | Transient | Navegación Shell (`PushAsync`, `GoToAsync`, `DisplayAlertAsync`), null-safe |
| `ICompassService` | `CompassSensorService` | Singleton | Lectura de brújula (`Compass.Default`) |
| `IOrientationService` | `OrientationSensorService` | Singleton | Lectura de orientación (`OrientationSensor.Default`) |
| `ICompassPositionService` | `CompassPositionService` | Singleton | Posición actual bajo demanda (`Geolocation.Default`) + permiso de ubicación |
| `IFlashlightService` | `FlashlightService` | Singleton | Control de flash (`Flashlight.Default`) |
| `IDeviceDisplayService` | `DeviceDisplayService` | Singleton | Control de brillo y `KeepScreenOn` |
| `IImagePickerService` | `ImagePickerService` | Singleton | Selección de imagen (`FilePicker`) |
| `IFilePickerService` | `FilePickerService` | Singleton | Selección de archivos (PDF/DOCX/XLSX) |
| `IDocumentPdfConverter` | `DocumentPdfConverter` | Singleton | Conversión DOCX/XLSX → PDF (DocIO/XlsIO) |
| `IFlashlightStateService` | `FlashlightStateService` (Core) | Singleton | Persistencia de estado flash entre recreaciones de VM, thread-safe con lock |
| `IStopwatchService` | `StopwatchService` (Core) | Singleton | Cronómetro con `Stopwatch` + `PeriodicTimer`, thread-safe con lock; persiste tiempo y marcas |
| `IMetronomeService` | `MetronomeService` | Transient | Metrónomo con `PeriodicTimer` y reproducción de audio |
| `IInstrumentAudioService` | `InstrumentAudioService` | Transient | Configuración de cuerdas, reproducción de audio, vibración |

**ViewModels**: Todos registrados como **Transient** (cada navegación obtiene una nueva instancia, evitando estado residual).

### 5.2 Lifetime de servicios

| Servicio | Lifetime | Justificación |
|----------|----------|---------------|
| `LanguageService` | Singleton | Sin estado persistente en memoria, seguro como Singleton |
| `ThemeService` | Singleton | Sin estado mutable |
| `DeviceStatusService` | Singleton | Lee datos en cada llamada, sin estado |
| `NavigationService` | Transient | Resuelve páginas desde DI, nuevo en cada llamada |
| `CompassSensorService` | Singleton | Comparte datos de sensores entre componentes |
| `OrientationSensorService` | Singleton | Comparte datos de sensores entre componentes |
| `CompassPositionService` | Singleton | Wrapper de `Geolocation.Default`, sin estado persistente entre lecturas |
| `FlashlightService` | Singleton | Wrapper de `Flashlight.Default` |
| `DeviceDisplayService` | Singleton | Wrapper de `DeviceDisplay.Current` |
| `ImagePickerService` | Singleton | Wrapper de `FilePicker` |
| `ScreenBrightnessService` | Singleton | Control de brillo nativo Android |
| `FlashlightStateService` | Singleton | Persiste estado flash entre recreaciones de VM |
| `StopwatchService` | Singleton | Persiste el estado del cronómetro (corriendo/pausado) y las marcas entre recreaciones de VM |
| `MetronomeService` | Transient | Timer y estado por instancia |
| `InstrumentAudioService` | Transient | Estado de audio y vibración por instancia, aislado por ViewModel |
| **Todos los ViewModels** | **Transient** | Cada navegación obtiene nueva instancia; evita estado residual entre sesiones |

---

## 6. Herramientas (Features)

### 6.1 Linterna (`FlashlightPage`)
- Encender/apagar linterna del dispositivo.
- **Intensidad variable (solo Android)**: control por `CameraManager.TurnOnTorchWithStrengthLevel`. El soporte se detecta leyendo `CameraCharacteristics.FLASH_INFO_STRENGTH_MAXIMUM_LEVEL`; solo se considera compatible si el máximo es mayor que 1. Esa clave existe desde Android 13, por lo que en versiones anteriores la función queda deshabilitada.
- El flash se enciende siempre al nivel elegido, incluso durante las señales SOS y HELP.
- **No interfiere con el sistema**: el nivel se lee de `CameraCharacteristics.FLASH_INFO_STRENGTH_DEFAULT_LEVEL` al iniciar la app y el valor elegido por el usuario solo vive en memoria mientras la app está abierta. Al apagar el flash, `CameraManager.SetTorchMode(false)` devuelve el hardware a su nivel por defecto de forma automática, sin intervención del código.
- Si el teléfono declara intensidad variable pero no declara un nivel por defecto, se usa el máximo.
- Cuando el dispositivo no es compatible, el control queda visible pero deshabilitado y se muestra un aviso en rojo (`MyDangerLight`/`MyDangerDark`).
- En iOS y demás plataformas se usa `Flashlight.Default` y no hay intensidad variable: la API pública solo permite encendido y apagado.
- Navegar a pantalla de luz completa (`ScreenLightPage`).
- Manejo de errores con reversión de estado.
- **Estado flash persistente**: `FlashlightStateService` (Singleton) mantiene `IsFlashOn` entre recreaciones de VM Transient.
- **Botones con converter**: `BoolToColorConverter` con `FalseColorLight`/`FalseColorDark` para soporte theme-aware. Reemplaza DataTriggers que no revertían correctamente el estilo base en MAUI.

### 6.2 Luz de pantalla (`ScreenLightPage`)
- Pantalla a brillo máximo y encendida.
- Control de brillo nativo solo en Android (`Window.Attributes.ScreenBrightness`).
- Restaura brillo original al salir.

### 6.3 Afinador / Instrumentos (`TunerPage`)
- Selección de instrumento: Guitarra Nylon, Guitarra Acero, Bajo, Ukulele, Violín, Charango.
- Cada instrumento tiene su propia página con componente reutilizable de cuerdas.
- Audio reproduction via `MediaElement` con clips WAV pregrabados.
- Animación de vibración en las cuerdas al tocar.

### 6.4 Metrónomo (`MetronomePage`)
- BPM ajustable (50-350).
- Firma de tiempos: 2/4, 3/4, 4/4, 5/4, 6/8, 7/8.
- Sonido de acento (1er tiempo) y normal (tiempos restantes).
- Implementado con `System.Timers.Timer` y `MainThread.BeginInvokeOnMainThread`.

### 6.5 Brújula (`CompassPage`)
- Lee sensor de brújula (`Compass.Default`) y orientación (`OrientationSensor.Default`).
- Dirección cardinal en 16 puntos (N, N-NE, NE, etc.).
- Indicador de inclinación del dispositivo (pitch/roll via quaternion).
- Navega hacia atrás si los sensores no son soportados.
- **Suavizado de ángulo**: Filtro exponencial (α=0.3) para lectura estable sin fluctuaciones rápidas. Maneja wrap-around 360°/0° correctamente.
- **Eficiencia de batería**: `SensorSpeed.UI` en vez de `SensorSpeed.Fastest`. Suficiente para actualización de UI, menor consumo.
- **Calibración manual**: Botón "Calibrar Brújula" que ejecuta flujo de 7 segundos (instrucción → calibrando → completado) con texto localizado. No se ejecuta automáticamente al entrar.
- **Protección contra crashes**: `OnNavigatedTo` síncrono con fire-and-forget seguro (try/catch). Unsubscribe antes de Stop() para evitar eventos post-limpieza.
- **Posición actual bajo demanda**: botón "Obtener mi posición" (`LocateCommand`) que consulta el GPS una sola vez, sin rastreo continuo. Grilla 2×2 con latitud, longitud, altitud y precisión horizontal.
  - Coordenadas en grados y minutos decimales con hemisferio: `S 47° 23.434'` / `W 123° 2.740'`. Formato **invariante de cultura** (`CultureInfo.InvariantCulture`) porque la coma decimal rompe la notación de coordenadas.
  - Altitud (`Location.Altitude`) y precisión (`Location.Accuracy`) son `double?`; `null` o `0` se muestra como "no disponible". La altitud real requiere hardware con barómetro/GPS compatibles.
  - Flujo: `IsAvailable` (`Geolocation.Default.IsEnabled`) → permiso `ACCESS_FINE_LOCATION` vía `Permissions.LocationWhenInUse` → `GeolocationRequest(GeolocationAccuracy.High, 20s)` con `RequestFullAccuracy = true` (Android 12+).
  - `IsLocating` deshabilita el botón y muestra "Buscando señal...". Errores diferenciados: ubicación desactivada, permiso denegado (`UnauthorizedAccessException`) y fallo genérico.
  - `StopSensors()` cancela el `CancellationTokenSource` de posición para no dejar lecturas pendientes al salir de la página.

### 6.6 Encuadre de imagen (`FramingPage`)
- Relaciones de aspecto: 1:1, 4:5, 9:16, 16:9.
- Modo de ajuste: AspectFill.
- Fondos: Blur, colores sólidos.
- Carga de imagen desde galería (`FilePicker`).

### 6.7 Manual (`ManualPage`)
- Página informativa estática.

### 6.8 Acerca de (`AboutPage`)
- Toggle de tema oscuro/claro con persistencia. En el **primer arranque** se aplica y persiste el tema oscuro (`ThemeService.ApplySavedTheme()` llama `SaveThemePreference(true)` cuando no hay preferencia guardada). El tema se aplica en `CreateWindow()` **antes** de crear la ventana (`App.xaml.cs`), garantizando dark desde el primer frame sin parpadeo de tema claro; la llamada ya no cuelga de `OnStart()`.
- **Versión** leída en runtime vía `IAppInfoService` (`AppInfo.Current`); única fuente de verdad: el `.csproj` (`ApplicationDisplayVersion`/`ApplicationVersion`). No duplicar versión en constantes.
- **Enlaces**: URLs centralizadas en `AppConstants.About` (Core) y abiertas con `ILauncherService` (`Launcher.Default`). El botón de donación solo se muestra si `DonationUrl` está configurada; el botón de repositorio fue eliminado.
- **Sitio web**: línea `© 2026 | francisco-dev.cl` completa como hipervínculo (un solo label con `TapGestureRecognizer`) hacia `AppConstants.About.WebsiteUrl`, con el color de texto del sistema (compatible tema claro/oscuro), igual que el label de versión (sin subrayado ni opacidad).
- **Versión 3 partes**: `ApplicationDisplayVersion=1.0.1` (texto libre válido en Android/iOS) y `ApplicationVersion=3` como build interno.

### 6.8.1 Indicadores de estado del menú (`MenuPage`)
- **Batería y almacenamiento se muestran como semáforo**: un `Ellipse` de 10 px junto a cada etiqueta, más el valor numérico a la derecha. El color **refuerza** la lectura, nunca la reemplaza: el número sigue visible porque el color por sí solo no es accesible para usuarios daltónicos.
- **Estados** (`StatusLevel`, enum en Core) mapeados a color del semáforo:

  | Estado | Color | Recurso |
  |--------|-------|---------|
  | `Ok` | Azul | `MyAccentBlue` `#2196F3` |
  | `Warning` | Naranja | `MyAccentOrange` `#FF990A` |
  | `Danger` | Rojo | `MyOn` `#FF0000` |
  | `Unknown` | Gris | `MyOff` (valor base, sin `DataTrigger`) |

  `Unknown` es el valor por defecto del `Ellipse` y se aplica cuando el dispositivo no entrega lectura, de modo que el punto nunca queda sin color definido.
- **Umbrales asimétricos por diseño**: la batería se evalúa por **porcentaje** y el almacenamiento por **bytes absolutos**.

  | Estado | Batería | Disco libre |
  |--------|---------|-------------|
  | Verde | ≥ 60% | ≥ 5 GB |
  | Naranja | 30-59% | 2-5 GB |
  | Rojo | ≤ 30% | < 2 GB |

  El almacenamiento **no** puede usar porcentajes: 20% libre puede ser correcto en un disco de 128 GB y crítico en uno de 16 GB.
- **El formateo a texto vive en `MenuViewModel`, no en el servicio.** El servicio devuelve números crudos porque XAML no puede comparar strings, y porque el umbral necesita la magnitud real. `FormatBytes()` y el mapeo a `StatusLevel` están en el VM.
- **`Core` no puede exponer colores de MAUI.** Por eso el VM expone el enum `StatusLevel` (agnóstico de plataforma) y el XAML lo traduce con `DataTrigger`; exponer `Color` desde Core rompería el build de los cuatro TFMs, igual que ocurrió con `MainThread`.
- **Se muestra solo el espacio libre**, no `usado / total`: la capacidad nominal de marketing (128 GB) no coincide con la partición real (~119 GB), y mostrar ambos invite a comparar con Ajustes y desconfiar. El total sigue disponible en `GetTotalStorageBytes()` por si se requiere.

### 6.9 Cronómetro (`StopwatchPage`)
- Iniciar/pausar/reiniciar con display `HH:mm:ss.mmm` (3 decimales) y **registro de marcas (vueltas)**.
- **Botones fijos** (no intercambian icono): `Play` (arranca; si ya corre, agrega una marca a la lista sin detener) y `Stop` (detiene; oprimido de nuevo limpia el cronómetro y la lista, dejando `00:00:00.000`).
- **Marcas**: `StopwatchLap` (Number, Split, Delta) en `ObservableCollection`; cada marca guarda el tiempo acumulado y la diferencia con la anterior, mostradas en una lista. Las marcas viven en `StopwatchService` (Singleton) y el VM las refleja en `Initialize()`.
- **Servicio Singleton en Core**: `StopwatchService` usa `System.Diagnostics.Stopwatch` + `PeriodicTimer` (intervalo 16 ms), thread-safe con lock. La UI se actualiza por evento `Tick` con `TimeSpan`.
- **Detención instantánea**: el ticker verifica cancelación antes de cada emisión y `Stop()` cancela antes de resetear, evitando que un tick residual "siga contando" tras pausar/reiniciar.
- **Marcas persistentes**: viven en el servicio Singleton (no en el VM Transient), por lo que persisten al salir y volver a la página y se limpian únicamente con el reset (segundo toque de Stop).
- **Estado persistente entre navegaciones**: al salir de la página el conteo continúa (patrón `FlashlightStateService`); `StopwatchViewModel.Initialize()` resincroniza al volver.
- **Borrado de una marca individual**: cada fila de la lista tiene un botón `icon_delete.png` al final que pide confirmación (`INavigationService.DisplayAlertConfirmAsync`, mismo patrón que Notas) y elimina solo esa marca.
- **Correlativos densos**: al borrar, `StopwatchService.RemoveLap(int index)` renumera para que no queden huecos y recalcula el delta de las marcas siguientes. `Number` es una propiedad almacenada e inmutable (`Number { get; init; }`), no un índice, y `StopwatchLap` no admite mutación, así que `RenumberLaps()` reconstruye las instancias. El delta es la distancia hasta la marca anterior **que existe** (`Split[i] - Split[i+1]`, la más antigua contra cero): borrar la marca 2 de [10 s, 25 s, 40 s] deja `2 · 00:00:40 · +30 s` y `1 · 00:00:10 · +10 s`.
- **El borrado opera por índice, no por referencia**: dentro de un `DataTemplate` el `BindingContext` es cada `StopwatchLap`, por lo que `CommandParameter="{Binding .}"` sí resuelve la marca, pero `Command="{Binding DeleteLapCommand}"` no encontraría el comando. Por eso el botón enlaza con `BindingContext.DeleteLapCommand` y `Source={x:Reference StopwatchPageRoot}`, igual que `NotesPage`.
- **Fondo de fila**: las filas usan `AppThemeBinding` con `MyPrimaryLight`/`MyPrimaryDark`, que es el fondo del estilo implícito de `Border` (`Styles.xaml`), y no `MyBackground`, que es un color fijo sin variación por tema.
- **Iconos**: botones fijos `icon_play.png` (Play/Marca) y `icon_stop.png` (Stop/Reset), sin `DataTrigger`. El asset `icon_stopwatch.png` se usa como icono del ítem del cronómetro en `MenuPage`.
- **Nota de dimensionado de íconos en `Button`**: según la documentación oficial de MAUI, los bitmaps **no se escalan** para ajustarse al `Button`; el tamaño recomendado del bitmap es de 32 a 64 unidades independientes del dispositivo. `Padding` solo define el espacio interior y `WidthRequest`/`HeightRequest` dimensionan el botón, ninguno redimensiona el ícono. Por eso el botón de borrar es de 44×44 con `Padding="10"`.
- **Nota de threading**: el `Tick` se dispara desde hilo background; validado en dispositivo, .NET MAUI refleja correctamente los cambios de propiedades bindables en la UI.

### 6.10 Guía del usuario (`GuidePage`)
- **Acceso**: desde `AboutPage` vía comando `OpenGuideCommand` (`INavigationService.PushAsync("GuidePage")`) y botón localizado `AboutOpenGuideText`.
- **Recurso multidioma**: assets `Resources/Raw/guide/USER_GUIDE.{es,en,sv}.md` (default español, fallback a `USER_GUIDE.es.md` si el idioma activo no existe). Se resuelven con `ILanguageService.GetCurrentLanguage()`. Los tres manuales mantienen la **misma numeración de secciones** (9 en total, la última es "Estado del dispositivo" con el semáforo del menú): al agregar una sección hay que actualizar el índice **y** los tres idiomas, porque el `Contenido` es un lista literal en cada archivo.
- **La sección 9 documenta los umbrales del semáforo** (batería por porcentaje, almacenamiento por GB libres absolutos) para que el manual de usuario y §6.8.1 no se desincronicen. La tabla de colores del manual es la versión legible para el usuario de la de §6.8.1, que además incluye los valores hexadecimales.
- **Render**: `IMarkdownToHtmlConverter` en Core con **Markdig 1.4.0** (`UseAdvancedExtensions`), imágenes embebidas como **data URI** (se leen de los assets y se inyectan en el HTML para que funcionen offline en el WebView), CSS de tablas para las dos columnas por feature y regex que captura tanto `![alt](archivo)` como `<img src="...">`.
- **Tema**: la guía era la única superficie visual sin soporte de tema, porque el WebView renderiza HTML plano y `AppThemeBinding` no aplica ahí. `ConvertToHtml(markdown, isDarkTheme, imageDataUris)` recibe el tema y elige entre dos paletas alineadas con `Resources/Styles/Colors.xaml` (`MyBackgroundLight/Dark`, `MyPrimaryTextLight/Dark`, `MyBackgroundMenuLight/Dark`); `Border` y `Muted` son las mezclas necesarias para tablas, citas y `hr`, que no existen en ese diccionario. **Descartado `prefers-color-scheme`**: ese media query refleja el modo oscuro del sistema operativo, no `Application.Current.UserAppTheme`, que la app gobierna con su propio toggle en `AboutPage`; con `prefers-color-scheme` la guía se desincronizaría del resto de la app al usar el toggle interno. La guía no usa enlaces ni bloques de código, así que no se estilan.
- **Recarga dinámica**: `GuidePage` se suscribe a `LanguageChanged` y a `ThemeChanged` en `OnNavigatedTo`, y se desuscribe de ambos en `OnNavigatingFrom`; al cambiar el idioma o el tema con la guía abierta, `LoadGuideAsync()` re-renderiza el HTML (`HtmlContent` del `GuideViewModel`). `ThemeChanged` lo emite `ThemeService.ApplyTheme` después de setear `UserAppTheme`, y el tema vigente se lee de `IThemeService.IsDarkTheme`.
- **Barras del sistema**: `ThemeService.UpdateStatusBarColors(bool isDarkMode)` aplica `MyBackgroundMenuDark`/`MyBackgroundMenuLight` de `Resources/Styles/Colors.xaml` según el tema (sin colores hardcodeados), resueltos con `ResolveThemeColor` sobre `Application.Current.Resources`. Además fija el contraste de los iconos con `WindowInsetsControllerCompat.AppearanceLightStatusBars`/`AppearanceLightNavigationBars`, porque `SetStatusBarColor` cambia solo el fondo y dejaría iconos blancos sobre fondo claro. Solo Android. `SetStatusBarColor`/`SetNavigationBarColor` están obsoletas desde Android 15+ (las apps dibujan de borde a borde por defecto), pero **en el dispositivo verificado el color de las barras sí cambia con esta llamada**, así que se mantienen. No se verificó el comportamiento en un Android 15+ real. En AndroidX Core 1.16 el contraste son **propiedades**, no métodos (`SetSystemBarsAppearance` no existe en esa versión). **Verificado en runtime** por el usuario en emulador Android (arranque desde Visual Studio): en tema oscuro la barra queda azul oscura con iconos claros, en tema claro queda casi blanca con iconos oscuros, y ambos cambian correctamente al cambiar el tema y al abrir la app.
- **Manejo de error**: si no se puede cargar, se muestra `GuideLoadErrorText` localizado.
- **DI**: `IMarkdownToHtmlConverter` (Singleton), `GuideViewModel` (Transient) y `GuidePage` (Singleton) registrados en `MauiProgram.cs`.
- **Identidad de app**: `ApplicationTitle` = "Navaja Suiza" y `ApplicationId` = `com.nefelin.navajasuiza` (consistente con `AndroidManifest.xml`; el README ya documentaba ese ID en el APK firmado).

---

## 7. Localización

### 7.1 Sistema implementado

Sistema de localización custom basado en archivos `.resx`:

- **Idiomas soportados**: Español (es-CL, default), Inglés (en-US), Sueco (sv-SE).
- **Archivos**: `AppResources.resx` (default), `AppResources.en.resx`, `AppResources.sv.resx`.
- **Persistencia**: Idioma guardado en `Preferences.Default` con key `app_language`.

### 7.2 Mecanismos

- **En XAML**: Markup extension `{extensions:Translate KeyText}` que vincula a `LocalizationResourceManager.Instance`.
- **En C#**: `LocalizationResourceManager.Instance["KeyText"]` o `ILanguageService.GetString("KeyText")`.
- **Cambio dinámico**: `LanguageService` dispara evento `LanguageChanged` que los ViewModels suscriben para actualizar UI.

### 7.3 Extensiones clave

- `LocalizationResourceManager`: Singleton que gestiona `CultureInfo` y accede a `AppResources.ResourceManager`.
- `TranslateExtension`: Markup extension XAML que crea un `Binding` al resource manager.

---

## 8. Modelos de datos

Ubicados en `NavajaSuiza.Core/Models/`:

### `InstrumentStringData`
```csharp
public class InstrumentStringData
{
    public string Note { get; set; }        // Nota musical (ej: "E2")
    public string AudioName { get; set; }   // Archivo WAV (ej: "GN_01_E2.wav")
    public string Description { get; set; } // Descripción (ej: "82.41 Hz")
    public int Thickness { get; set; }      // Grosor de la cuerda (para UI)
}
```

### `SupportedLanguages`
Constantes estáticas para códigos de idioma: `"es"`, `"en"`, `"sv"`.

### `AppConstants`
Constantes centralizadas agrupadas por dominio (Metronome, Framing, Instruments).

---

## 9. Componentes reutilizables

| Componente | Propósito |
|-----------|-----------|
| `InstrumentStringComponent` | Cuerda individual de instrumento con propiedades bindables (Note, AudioName, Description, Thickness), registro de audio y vibración |
| `InstrumentBody` | Cuerpo visual del instrumento (ContentView vacío, solo wrapper) |
| `LoadingComponent` | Indicador de carga |

---

## 10. Convertidores

| Converter | Función |
|-----------|---------|
| `BoolToColorConverter` | Convierte `bool` a color (TrueColor/FalseColor). Soporta `FalseColorLight`/`FalseColorDark` para theme-aware |
| `InvertedBoolConverter` | Invierte `bool` (true→false, false→true). Usado para `!IsBusy` en bindings de IsEnabled |
| `StringNotEmptyConverter` | `string` no vacío/no blanco → `true`. Usado para la visibilidad de mensajes (error, vacío, confirmación). `ConvertBack` lanza `NotSupportedException` |

---

## 11. Assets de audio

32 archivos WAV en `Resources/Raw/`:

| Prefijo | Instrumento | Cantidad |
|---------|-------------|----------|
| `GN_` | Guitarra Nylon | 6 |
| `GS_` | Guitarra Acero | 6 |
| `B_` | Bajo | 4 |
| `U_` | Ukulele | 4 |
| `V_` | Violín | 4 |
| `C_` | Charango | 6 |
| `tik_` | Metrónomo | 2 |

---

## 12. Plataformas y permisos

### Permisos Android (`AndroidManifest`)
- `CAMERA`
- `FLASHLIGHT`
- `ACCESS_COARSE_LOCATION` / `ACCESS_FINE_LOCATION` (posición bajo demanda, ver 6.5)
- `WRITE_EXTERNAL_STORAGE` con `android:maxSdkVersion="28"` (solo para exportar la pizarra en Android API 21–28; ver 18.1)

**Ubicación**: re-Agregada a propósito para la función "Mi posición" de la brújula (6.5), revirtiendo parte de la tarea 33. Se solicita en runtime solo al presionar el botón y únicamente mientras la app está en uso; no hay rastreo en background. Google Play requiere declarar el uso de ubicación en el Data safety del release.

**`INTERNET`**: lo re-inyecta `CommunityToolkit.Maui` en el manifest fusionado (`obj/.../AndroidManifest.xml`); permiso *normal*, se conserva.

**No declarados** (principio de mínimo privilegio, eliminados): `BATTERY_STATS` (protegido, no se usa; ver batería abajo), `ACCESS_NETWORK_STATE`, `INTERNET` (en el manifest fuente).

### Lectura de batería sin permisos
MAUI `Battery.Default` en Android exige `BATTERY_STATS` (permiso protegido `signature|privileged`, red flag en Play). `DeviceStatusService.GetBatteryLevel()` en Android usa `BatteryManager.GetIntProperty(Android.OS.BatteryProperty.Capacity)` (API 21+, síncrono, sin permiso); `Battery.Default` queda como fallback en otras plataformas.

### ABIs / empaquetado Android (`RuntimeIdentifiers`)
- `AndroidSupportedAbis` quedó **obsoleta** (warning XA0036) en .NET 10 / Android SDK 36; se reemplazó por `RuntimeIdentifiers` condicionados al target Android: `android-arm;android-arm64;android-x64` → `armeabi-v7a` (32-bit), `arm64-v8a`, `x86_64`. Un solo APK cubre todos los dispositivos; Play usará AAB.
- Dispositivos budget pueden correr Android **solo 32-bit** (caso real: Galaxy A11 `SM-A115M`, Android 12, `ro.product.cpu.abi=armeabi-v7a`); un APK sin `armeabi-v7a` falla con "app no compatible" (`INSTALL_FAILED_NO_MATCHING_ABIS`).
- Los builds **Debug** de MAUI apuntan a `x86_64` (emulador): no instalar en teléfonos reales. Validar ABI con `adb shell getprop ro.product.cpu.abi`.

### Soporte mínimo por plataforma
| Plataforma | Versión mínima |
|-----------|---------------|
| Android | API 21 (5.0) |
| iOS | 15.0 |
| macOS | 15.0 |
| Windows | 10.0.17763.0 |

---

## 13. Decisiones de diseño

| Decisión | Razón |
|----------|-------|
| MVVM con CommunityToolkit | Ecosistema MAUI estándar, source generators para boilerplate |
| Shell navigation | Navegación nativa MAUI con soporte para rutas y TabBar |
| Core + MAUI (separación) | Interfaces compartidas en Class Library, implementaciones en MAUI |
| Servicios Singleton vs Transient | Singleton para servicios sin estado mutable persistente (sensores, wrappers de APIs de plataforma); Transient para servicios con estado por instancia (audio de instrumentos, metrónomo, ViewModels) |
| Localización por .resx | Mecanismo nativo .NET, soporte XAML y C#, fallback automático |
| Audio via MediaElement | Componente nativo del CommunityToolkit.Maui con soporte multiplataforma |
| `StopwatchService` Singleton + UI por evento `Tick` | Lógica de cronómetro 100% pura (testeable) en Core; la VM Transient se suscribe/resincroniza en cada navegación |
| `allowBackup="false"` en Android | Las Notas son datos locales sensibles; sin respaldo automático ni transmisión (formulario Data Safety simple). Evaluado SQLCipher para cifrado en reposo, descartado por ahora: cifrar sin poder exportar la clave impide restaurar notas en otro dispositivo; respaldar la clave anula la protección |

---

## 14. Plan de optimización

### Fase A — Corrección de issues de código (SENIOR)

| # | Issue | Archivo(s) | Severidad | Estado |
|---|-------|-----------|-----------|--------|
| 0 | Renombrar `PagesViewModel/` → `ViewModels/` | Carpeta + referencias | Baja | ✅ Completado |
| 1 | `CompassViewModel` no extiende `BaseViewModel` | `CompassViewModel.cs` | Alta | ✅ Completado |
| 2 | `ScreenLightViewModel.Cleanup()` oculta al padre sin `override` | `ScreenLightViewModel.cs` | Alta | ✅ Completado |
| 3 | Logs copy-paste en 5 ViewModels de instrumentos | `Instrument*ViewModel.cs` | Media | ✅ Completado |
| 4 | `ImageProcessingService.ProcessImageAsync` lanza `NotImplementedException` | `ImageProcessingService.cs` | Media | ✅ Completado (servicio eliminado) |
| 5 | Código muerto/comentado en múltiples archivos | Varios | Baja | ✅ Completado |
| 6 | Carpeta `Behaviors/` vacía | `.csproj` | Baja | ✅ Completado |
| 7 | ViewModels Singleton → Transient | `MauiProgram.cs` + ViewModels | Alta | ✅ Completado |
| 8 | `InstrumentStringComponent` resuelve DI manualmente | `InstrumentStringComponent.xaml.cs` | Media | ✅ Completado (AudioService via BindableProperty) |
| 9 | `MetronomeService` usa `System.Timers.Timer` | `MetronomeService.cs` | Baja | ✅ Completado (migrado a PeriodicTimer) |
| 10 | `B_00_B0.wav` eliminado | `Resources/Raw/` | Baja | ✅ Completado (commit 7248906) |
| 16 | CompassPage crash Android | `CompassPage.xaml.cs`, `CompassViewModel.cs` | Alta | ✅ Completado |
| 17 | Compass calibración automática | `CompassViewModel.cs`, `CompassPage.xaml` | Media | ✅ Completado |
| 18 | Audio de instrumento no se detiene al navegar fuera | `Instrument*Page.xaml.cs` | Alta | ✅ Completado |

### Fase B — Documentación

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 11 | Actualizar `README.md` con estructura real | `README.md` | ✅ Completado |

### Fase C — Separación Core + MAUI

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 12 | Crear proyecto `NavajaSuiza.Core` (Class Library) | `.csproj` + estructura | ✅ Completado |
| 13 | Migrar interfaces compartidas | Core | ✅ Completado |
| 14 | Migrar `SupportedLanguages` a Core | Core/Models | ✅ Completado |
| 15 | Migrar `BaseViewModel` a Core | Core/ViewModels | ✅ Completado |
| 16 | Eliminar duplicados en MAUI | MAUI/Services/Interfaces | ✅ Completado |
| 17 | Actualizar referencias en proyecto MAUI | `.csproj` + usings | ✅ Completado |
| 18 | Verificar build completo | `dotnet build` | ✅ Completado (0 errores) |
| 19 | Migrar ViewModels testables a Core | Core/ViewModels | ✅ Completado (12 ViewModels) |
| 20 | Crear INavigationService en Core | Core/Interfaces | ✅ Completado |
| 21 | Migrar IInstrumentAudioService/IMetronomeService a Core | Core/Interfaces | ✅ Completado (abstracted con object) |
| 22 | Migrar InstrumentStringData a Core | Core/Models | ✅ Completado |
| 23 | Crear AppConstants centralizado | Core | ✅ Completado |
| 24 | Crear NavigationService en MAUI | MAUI/Services | ✅ Completado |
| 25 | Crear proyecto de tests | NavajaSuiza.Test | ✅ Completado (81 tests) |

### Fase D — Enriquecimiento de SKILL.md

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 26 | Agregar sección MAUI completa al SKILL (transversal) | `SKILL.md` | ✅ Completado |

### Fase E — Cierre de optimización

| # | Tarea | Archivo(s) | Estado |
|---|-------|-----------|--------|
| 27 | Renombrar constantes a UPPER_SNAKE y alinear `.editorconfig` | Core + tests | ✅ Completado |
| 28 | Consolidar `SupportedLanguages` en Core (eliminar duplicado MAUI) | Core/Models + MAUI | ✅ Completado |
| 29 | Eliminar dead code (asset `test.png`, botones Notes/Weather sin Command, typo "Hz Hz", botón TEST, botón "Acerca de" del menú) | Varios | ✅ Completado |
| 30 | Ampliar tests a servicios y ViewModels de instrumentos/tuner (`FlashlightStateService`, `Instrument*ViewModel`, `TunerViewModel`) | `NavajaSuiza.Test` | ✅ Completado (81 tests) |
| 31 | Corregir CI: rama disparadora `main` → `master` + ejecutar solo tests (build MAUI no viable en Windows) + actions `@v5` | `.github/workflows/build.yml` | ✅ Completado (run verde 81/81) |
| 32 | Alinear documentación con el código (lifetimes, conteo de ViewModels, bug #18/19) | `DEVELOPMENT.md`, `README.md`, `SKILL.md` | ✅ Completado |

**Cierre**: La fase de optimización del proyecto queda cerrada. Deuda técnica conocida y documentada: los servicios MAUI (`LanguageService`, `MetronomeService`, `InstrumentAudioService`, etc.) no tienen tests automatizados de forma deliberada (requeriría un test project MAUI de costo elevado para una app de este alcance). La verificación runtime fue realizada por el usuario en dispositivo y validada por los tests automatizados en CI.

### Fase F — Play Store readiness (release)

| # | Tarea | Archivo(s) | Estado |
|---|-------|-----------|--------|
| 33 | Reducir permisos Android a mínimo (quitar `BATTERY_STATS`, location, network, `INTERNET` del source) | `Platforms/Android/AndroidManifest.xml` | ⚠️ Parcialmente revertido: `ACCESS_FINE_LOCATION`/`ACCESS_COARSE_LOCATION` re-agregados para la posición bajo demanda de la brújula (6.5). `BATTERY_STATS`, `ACCESS_NETWORK_STATE` y `INTERNET` siguen fuera. |
| 34 | Empaquetado multirarquitectura (RIDs arm/arm64/x64; A11 32-bit compatible) | `NavajaSuiza_.NET10.csproj` | ✅ Completado (APK Release fat 57,9 MB, 3 ABIs) |
| 35 | Batería sin `BATTERY_STATS` vía `BatteryManager` (Android) | `DeviceStatusService.cs` | ✅ Completado |
| 36 | Cronómetro: marcas persisten entre navegaciones (servicio Singleton) | Core (`Stopwatch*`) + tests | ✅ Completado (144 tests) |
| 37 | Guía del usuario multi-idioma y multi-tema (Markdig, assets `USER_GUIDE.{es,en,sv}.md`, recarga al cambiar idioma o tema) | Core + MAUI (GuidePage) | ✅ Completado (214 tests) |
| 38 | Identidad de app para la tienda (`ApplicationId com.nefelin.navajasuiza`, título "Navaja Suiza") | `NavajaSuiza_.NET10.csproj` | ✅ Completado |

**Estado de release (2026-09-30)**: app **publicada en Google Play, en Internal testing y Closed testing** (pista `Prueba cerrada - Alpha`, release `1.1.97-prueba-cerrada`, AAB firmado aceptado; la vía de Internal testing está activa y se puede subir una versión nueva cuando haga falta). Privacy Policy **publicada** en `https://www.francisco-dev.cl/navaja-suiza/privacy-policy` (fuente en `PRIVACY_POLICY.md`, trilingüe + bloque Astro), **`allowBackup=false` decidido** (Notas solo locales, sin transmisión) y targetSdk 36 (cumple). **Siguiente paso**: completar los 12 testers opted-in durante 14 días continuos en la pista cerrada y luego solicitar la revisión de producción.

---

## 15. Hallazgos y deuda técnica

### 15.1 Bugs corregidos

1. **`CompassViewModel` no extiende `BaseViewModel`**: Corregido — ahora extiende `BaseViewModel` correctamente.
2. **Logs copy-paste en ViewModels de instrumentos**: Corregido — cada ViewModel ahora tiene su propio mensaje correcto.
3. **`ScreenLightViewModel.Cleanup()` oculta al padre sin `override`**: Corregido — ahora usa `override` y llama a `base.Cleanup()`.
4. **`InstrumentStringComponent` resuelve DI manualmente**: Corregido — ahora usa `BindableProperty AudioService` con binding desde XAML.
5. **Código muerto/comentado**: Eliminado en ThemeService, InstrumentNylonViewModel, MetronomeViewModel, MetronomeService, IMetronomeService, AppShell, FlashlightViewModel, MenuPage.
6. **`ImageProcessingService` e `IImageProcessingService`**: Eliminados completamente (servicio sin implementación funcional).
7. **SkiaSharp packages**: Eliminados (sin uso).
8. **Carpeta `Behaviors/` vacía**: Eliminada.
9. **Carpeta `PagesViewModel/` renombrada** a `ViewModels/`.
10. **UTF-8 encoding corregido** en archivos que tenían caracteres corruptos.
11. **FlashlightPage DataTrigger stuck**: DataTriggers no revertían estilo base al desactivarse. Reemplazados por `BoolToColorConverter` con Binding directo y soporte theme-aware (`FalseColorLight`/`FalseColorDark`).
12. **FlashlightViewModel Transient sin persistencia**: `IFlashlightStateService` (Singleton) ahora persiste estado `IsFlashOn` entre recreaciones de VM.
13. **NavigationService null warnings**: CS8602/CS8604 por desreferencias posiblemente null. Separado en dos pasos con null checks explícitos.
14. **TunerPage re-entrancy**: Clicks rápidos en botones de instrumentos creaban múltiples instancias. Solucionado con guard `IsBusy` + `InvertedBoolConverter` para deshabilitar botones durante navegación.
15. **FlashlightStateService sin thread-safety**: Agregado `lock` para proteger acceso concurrente a `IsFlashOn`.
16. **CompassPage crash en Android**: Navegar desde brújula a About y volver a Menú causaba `JavaProxyThrowable`. Causa: `async void OnNavigatedTo` con `await` de sensores + suscripciones duplicadas a eventos de sensores. Solución: `OnNavigatedTo` síncrono con fire-and-forget seguro, VM cacheado, unsubscribe antes de Stop().
17. **Compass calibración automática innecesaria**: Calibración se ejecutaba cada vez que se abría la brújula con `Task.Delay` de 7 segundos, causando UX deficiente. Solución: calibración manual con botón + feedback visual con mensajes localizados.
18. **Audio de instrumento no se detiene al navegar/cambiar de tab**: El `MediaElement` del Page Singleton podía retener audio al cambiar de tab Shell o al re-navegar a un instrumento. Se evaluó cambiar `InstrumentAudioService` a Singleton, pero se descartó (rompe el aislamiento de estado por instancia). **Solución final**: llamada a `TunerMediaElement.Stop()` en `OnNavigatedTo` de cada página de instrumento (antes de registrar el nuevo MediaElement) + limpieza en `OnDisappearing` (`StopAllStringAsync`, `ClearStringBorders`). El servicio permanece **Transient**.

19. **AboutPage navigation crash en Android**: `GoToAsync("//AboutPage")` causaba `JavaProxyThrowable` en Android. El botón interno del menú era redundante con el tab inferior. Se eliminó el botón, `NavigateToAboutCommand` y el workaround `IsDevelopment` asociado. El acceso a About queda por el tab inferior del TabBar.

20. **`TestingPage`/`TestingViewModel` vacíos**: Página de pruebas sin implementación, botón TEST oculto por `IsDevelopment`. Eliminada junto con sus registros DI, entry de `.csproj` y botón de menú.
21. **Almacenamiento del menú nunca mostraba el valor real**: `DeviceStatusService` consultaba `Environment.ExternalStorageDirectory` (`/storage/emulated/0`), que con **almacenamiento por ámbito** no es confiable, y el `catch` mudo devolvía `"N/A"` sin explicar la causa. Corregido — ahora usa `Environment.DataDirectory` (partición `/data`, la que el usuario ve en Ajustes) en Android y `FileSystem.Current.AppDataDirectory` + `DriveInfo` en iOS/MacCatalyst/Windows, donde antes devolvía siempre `"N/A"`. Los `catch` ahora loguean con `_logger.LogWarning(ex, ...)`.
22. **Batería del menú marcaba mal**: `Battery.Default.ChargeLevel` usaba `level > 0`, lo que excluía `0%` y mostraba `"N/A"` con la batería agotada. Corregido a `level >= 0`. Además el `int?` implícito de `GetIntProperty` ahora coalesce a `-1`.
23. **Valores por defecto mentirosos del menú**: `AvailableStorage` arrancaba en `"0 GB"` (afirmaba cero espacio libre) y `BatteryLevel` en `"0%"`. Ambos ahora `"N/A"` con `StatusLevel.Unknown`, que distingue "no lo sé" de "cero".

21. **Batería no visible tras la limpieza de permisos**: MAUI `Battery.Default` exige `BATTERY_STATS` en Android. Solución: lectura con `BatteryManager.GetIntProperty(BatteryProperty.Capacity)` (sin permiso). `BATTERY_STATS` NO se re-agrega (permiso protegido, red flag en Play).

22. **Cronómetro: las marcas se perdían al navegar**: `Laps` vivía en el ViewModel (Transient). Solución: persistencia en `StopwatchService` (Singleton); las marcas se limpian únicamente con el reset (segundo toque de Stop).

23. **Metrónomo: jitter y deriva acumulada del clic**: el scheduler usaba `PeriodicTimer` con salto al UI thread, lo que acumulaba deriva. Solución: scheduler de tiempos absolutos con `Stopwatch` (`Stopwatch.StartNew()` + `nextTick - stopwatch.Elapsed`) y `Android.Media.SoundPool` para la reproducción; `MediaElement` queda solo como fallback cuando `SoundPool` no está disponible. **Sin dependencias nuevas.** No se extrajo un `MetronomeClickService` por plataforma: la latencia quedó resuelta sin necesidad de una capa nueva. Referencia: jfversluis/Plugin.Maui.Audio#89 documenta latencia de 150-200 ms incluso con player precargado. **Verificado en runtime** por el usuario en emulador Android (arranque desde Visual Studio): clic normal y acentuado auditionados sin deriva.

24. **Despliegue en emulador desde Visual Studio se agotaba esperando el proceso**: el lanzador expiraba con "No se pudo obtener el id. de proceso para 'com.nefelin.navajasuiza'" durante el arranque en frío (~6 s) combinado con Fast Deployment (`monodroid-debug: Not starting the debugger as the timeout value has been reached`). **Diagnóstico anterior corregido**: la causa **no** era que `adb` estuviera fuera del PATH. Verificado en esta máquina: `adb` sigue fuera del PATH (ni en variables de sistema ni de usuario, y sin `ANDROID_HOME`/`ANDROID_SDK_ROOT`) y el despliegue funciona igual, porque Visual Studio resuelve el Android SDK por su cuenta. El fallo era **intermitente**, del mismo tipo que el bloqueo de Smart App Control. **Verificado**: la app despliega y arranca desde Visual Studio sin pasos manuales.

25. **Limpieza de código muerto, catches silenciosos y recursos huérfanos** (complementa el punto 5):
    - `FlashlightViewModel`: eliminados la propiedad `IsLightOn` y la dependencia única de `ILanguageService`, con su suscripción a `LanguageChanged` y su `Cleanup()` vacío. `FlashlightPage.xaml` ya no declara `BoolToStatusText` ni `BoolToActionText`.
    - `BoolToLocalizedStringConverter` eliminado: quedó huérfano al quitar esas dos declaraciones. Sus etiquetas de linterna tenían los valores **invertidos** (`FlashlightStatusOffText` = "Encendido") y `FramingModeFillText` era una etiqueta obsoleta, ya que el modo `AspectFill` se muestra con `FramingModeCoverText`.
    - Catches silenciosos con traza: `FlashlightViewModel` ahora recibe `ILogger<T>` para los fallos de apagado antes y después de Morse; `CompassPage.xaml.cs` y `ThemeService` usan `Debug.WriteLine`. Se conservan los `catch (OperationCanceledException)` y los fallbacks deliberados.
    - 8 claves sin uso eliminadas de `AppResources.resx`, `AppResources.en.resx` y `AppResources.sv.resx`: `FlashlightActionOnText`, `FlashlightActionOffText`, `FlashlightStatusOnText`, `FlashlightStatusOffText`, `FramingModeFillText`, `PdfReaderEmptyHintText`, `CompassCalibrationIconText` y `WeatherText`. Las 3 tablas quedan alineadas con 124 claves cada una, con BOM UTF-8 y CRLF preservados.

### 15.2 Issues pendientes (Backlog)

- **Biblioteca de componentes MAUI**: la planificación se extrae a un proyecto independiente (no entra en el alcance de esta app). El documento de planificación se movió fuera del repositorio.
- **Ripple del cronómetro al registrar una vuelta**: abierto. `AppConstants.Stopwatch.TICK_INTERVAL_MS` está en **16 ms** (~60 actualizaciones de `ElapsedText` por segundo). El cambio desde 10 ms **no resolvió el síntoma y empeoró la percepción**: con menos repintados pero más espaciados, cada salto de tiempo se hace más visible. Por tanto el intervalo **no es la causa raíz** y subirlo más no es el camino. Observación clave del usuario: el contador **no se atrasa** (el valor siempre es correcto), solo **no es fluido**, lo que descarta un backlog de valores obsoletos. Hipótesis pendiente de medir: el `await` de `RunTickerAsync` (`StopwatchService.cs:147`) no usa `ConfigureAwait(false)`, por lo que — pese al fire-and-forget de la línea 103 — el ciclo completo (`Tick` → `OnTick` → `ElapsedText`) podría ejecutarse en el **hilo de UI**, compitiendo con la inflación de la fila que hace `Laps.Insert(0, lap)` al marcar. Eso explicaría la falta de fluidez sin atraso. **Sin medir**: se propuso registrar `Thread.CurrentThread.Name` en `OnTick` para confirmar en qué hilo corre, y no se ha ejecutado. No se ha aplicado coalescencia de updates ni `ConfigureAwait(false)`. **No es una regresión del borrado de marcas** (el diff de `StopwatchService` solo agrega `RemoveLap`/`RenumberLaps`, fuera de la ruta del tick). Nota: el emulador renderiza por software y amplifica el síntoma, así que la validación final requiere un dispositivo físico.
- **Disposición del `CancellationTokenSource` del ticker**: `StopwatchService` hace `cts?.Dispose()` mientras el `PeriodicTimer` puede tener un `WaitForNextTickAsync` pendiente, lo que puede lanzar `ObjectDisposedException` en el lazo fire-and-forget y dejar el ticker muerto. No reproducido hasta ahora; pendiente de revisar.
- **Pruebas sin poder ejecutar por Smart App Control**: `dotnet test` falla con `System.IO.FileLoadException ... (0x800711C7)`, "Una directiva de Control de aplicaciones bloqueó este archivo", al cargar `NavajaSuiza.Core.dll` en el constructor de las clases de prueba. **No es un fallo del código bajo prueba**: el build compila los cuatro TFMs con 0 advertencias. Es transitorio y depende de la reputación que Windows asigne al binario recién compilado; una corrida anterior sí completó 230/230 sin cambios en el proyecto. Mitigaciones descartadas: `Unblock-File` no sirve porque el archivo **no** tiene `Zone.Identifier` (no es Mark of the Web). Workaround: reintentar, o ejecutar las pruebas con el proyecto recién compilado. **Los 15 tests de `MenuViewModel` (incluidos los del semáforo) están escritos pero no verificados en la última corrida.**
- **Conteo de casos de prueba inconsistente**: el runner reporta 229, 230 y 232 en distintas corridas. La causa es la aforementioned carga fallida del assembly (los constructores fallan y alteran el conteo), no una diferencia real de tests. La cifra esperada es la que se obtiene con la suite completa en verde.
- **Sin pruebas directas de `DeviceStatusService`**: la lógica de `StatFs`/`DriveInfo` y el mapeo de umbrales no tienen cobertura propia; `MenuViewModel` se prueba con mocks de `IDeviceStatusService`. Probar el servicio real requiere Android APIs y no es trivial, por lo que queda pendiente decidir si vale la pena.
---

## 16. Referencias

- **SKILL.md**: Guía transversal de patrones .NET Senior (APIs REST + MAUI).
- **AGENTS.md**: Reglas de operación para OpenCode en este proyecto.
- **README.md**: Documentación general del proyecto (estructura, arquitectura y uso).
- **NavajaSuiza.Core**: Class Library con interfaces y modelos compartidos.

---

## 17. Publicación en Google Play

Pasos para publicar `NavajaSuiza` en Google Play (proceso de mantenedor, no documentación de usuario):

1. **Redactar la política de privacidad**: texto trilingüe (es/en/sv) en `PRIVACY_POLICY.md`. **`allowBackup=false` decidido** (19/09/2026): las Notas solo viven en el dispositivo; no hay transmisión que declarar por respaldo. Hacer lo mismo para Data Safety.**✅ Hecho**
2. **Publicar la política en la web**: subir el texto a una página pública de `francisco-dev.cl` (ej. `.../privacy-policy`) y obtener la URL. **✅ Publicado en `https://www.francisco-dev.cl/navaja-suiza/privacy-policy`**
3. **Pegar la URL en Play Console**: en el listing, campo "Política de privacidad" → la URL pública (Play solo recibe el link, no archivos).
4. **Completar el formulario Data Safety**: dentro de Play Console, responder casillas coherentes con el comportamiento real (Notas = datos del usuario solo locales; cámara = solo linterna, sin captura; batería/sensores = lectura local; sin anuncios/analytics/servidores).
5. **Subir assets del listing**: ícono 512×512, screenshots (mín. 2, recomendado 6–8), textos trilingües ES/EN/SV.
6. **Cargar el Release AAB firmado**: subir el `.aab` (ver README §Release) → Internal testing → Closed testing → Production.

Pasos 1–6 completados: la app está publicada en Internal testing y Closed testing. Estado actual y siguiente paso en §14 Fase F.

---

## 18. Features planadas (backlog de desarrollo)

### 18.1 Pizarra (dibujo)

Estado: **Fases 1 y 2 implementadas y verificadas** (suite total 230 tests; builds Android/Windows 0 errores). Fase 2 = export WebP a galería, ahora **transversal vía SkiaSharp** (verificado en emulador Android y en Windows). **Goma descartada**: Deshacer (LIFO) + Limpiar cubren el caso de esta app.

Entregado (Fase 1):
- Lienzo a máximo espacio (`Grid` `Auto,Auto,*`), **sin `ScrollView`** (interceptaba los gestos verticales del dibujo).
- Herramientas: lápiz con grosor inline (slider 1–18), **limpiar todo** y **deshacer** (botones en fila junto a Colores/Pizarra).
- **Paleta de 9 colores en diálogo** overlay (sin fila inline → sin overflow de su ancho), abierto desde el botón "Colores".
- Selector de fondo de pizarra **Dark/Light** (action sheet) desde el botón "Pizarra".
- **Adaptación automática de contraste del lápiz** al cambiar de pizarra (WCAG ≥ 2.5:1): negro sobre oscura → amarillo; blanco sobre clara → negro; rojo (default) se conserva visible en ambas.
- `PizarraStroke`/`PizarraPoint` y `PizarraViewModel` en Core (comandos testables: `Clear`, `Undo`, `SetBoardColor`, `OpenColorPicker`/`CloseColorPicker`/`SelectColor`); `StrokeDrawable` (`IDrawable`) y `PizarraPage` en MAUI.
- Ruta `"PizarraPage"`, ítem de menú con `icon_pizarra.png`, localización es/en/sv.

Entregado (Fase 2 — Guardar/export a galería):
- **Decisión**: se descartó la persistencia JSON propuesta originalmente (generaba archivos grandes y opacos para el usuario; "no todos saben qué es un JSON"). `Guardar` **exporta el dibujo como imagen WebP a la galería** (comprensible y borrable por el usuario). La pizarra **abre siempre en blanco** (sin restauración).
- **SkiaSharp 4.152.1** (única dependencia nueva, instalada por el usuario): `IPizarraImageExporter` (Core) + `PizarraImageExporter` (MAUI) con **raster + encode transversal** — una sola implementación para todas las plataformas usando `SKBitmap`/`SKCanvas`/`SKPathBuilder`/`SKPaint` (caps/joins redondos, antialias, mismo estilo que `StrokeDrawable`) y `SKImage.Encode(Webp, 95)`. Reemplaza el raster nativo de Android (`Bitmap`/`Canvas`/`Paint`) y corrige de paso un bug: las coordenadas de trazo ahora **se escalan** junto con el grosor (antes solo se escalaba el grosor).
- Layout de la imagen: máx. 2048px con margen (padding 40 + media anchura de trazo), `scale ≤ 1` (downscale si el contenido excede; sin upscale para no degradar).
- Guardado por plataforma: **Android** vía `MediaStore.Images` → `Pictures/pizarra.webp` (API 29+ sin permiso; API 21–28 pide `WRITE_EXTERNAL_STORAGE` en runtime, declarado en manifest con `maxSdkVersion="28"`); **Windows** → archivo WebP en Carpeta de imágenes (nombre único con fecha+guid); iOS/MacCatalyst `NotAvailable`.
- El VM expone `PizarraExportResult` (`Saved`/`NotAvailable`/`Failed`) y la página muestra el mensaje correspondiente (resx ×3).

Fase 3 **descartada por decisión del usuario** (2026-09-30): no habrá botón de compartir, porque la imagen se guarda localmente en la galería y eso ya cubre el caso de uso. Tampoco se implementa el guardado en Photos de iOS, ya que el proyecto no cubre iOS por no disponer de Mac para compilar. La Pizarra queda **completa en las Fases 1 y 2**.

Enfoque técnico de export (referencia):
- **MAUI no exporta `GraphicsView` a archivo**: se re-rasterizan los trazos desde el modelo. Ahora con **SkiaSharp** (una dependencia, raster + WebP transversal para todas las plataformas). Se evaluó un rasterizador propio en Core + encoder PNG (0 dependencias) pero se descartó: más líneas de gráficas que mantener y se perdía el WebP liviano.
- **`MediaSaver`/`FileSaver` de CommunityToolkit no existen en v15.0.1** (verificado en el cache del paquete), por lo que la escritura a galería usa `MediaStore` nativo (solo esa parte es de plataforma).
- SkiaSharp habilita además: export a imagen de futuros dibujos, herramientas de figuras/texto en la Pizarra y gráficos propios (ej. historial del Contador de pasos).

### 18.2 Contador de pasos

Estado: **planificada, sin urgencia** — el usuario confirmó que le serviría, pero no es prioritario. Enfoque definido; pendiente confirmar alcance y autorización para implementar.

Referencia de cómo funciona (contexto técnico):
- El conteo real **no usa GPS**; usa sensores inerciales:
  - **Android `TYPE_STEP_COUNTER`** (API 19+): el SoC/coprocesador de movimiento detecta pasos; entrega cuenta acumulada desde el último reboot. **Sin permiso** y batería mínima; funciona con la app cerrada (lo usan Google Fit/Samsung Health).
  - Base del mecanismo: acelerómetro con detección de picos de la marcha (~1–3 Hz) filtrada con umbrales/suavizado.
  - **iOS:** `CMPedometer`/HealthKit sobre el coprocesador de movimiento; requiere `NSMotionUsageDescription`.
  - **Windows:** sin sensor de pasos nativo → mostrar "no disponible".
- Precisión: buena con sensor dedicado; el error sube con heurísticas (brazos, vehículo).

Enfoque en esta app:
- Servicio interno primero (patrón `Core/Interfaces` + `Services/Implementations`), Android `STEP_COUNTER` como primera iteración; iOS `CMPedometer` después.
- Respaldo del conteo en `Preferences`/JSON local (permite conteo diario con `allowBackup=false`).
- **No crear NuGet por ahora**: el paquete comunitario sería un proyecto aparte (nuevo), solo tras validar el servicio en dispositivos reales (decisión en §13).

Pendiente de definir: ¿conteo solo en primer plano o en segundo plano/cerrada?; ¿historial de días?; ¿reset a medianoche?; ¿dónde se muestra (página propia o dentro de otra)?

### 18.4 Visor de PDF (Syncfusion SfPdfViewer)

Estado: **implementada y verificada en dispositivo real** (Android/Windows build 0/0; suite total 230 tests; **todas las conversiones y los visores probados en runtime en dispositivo físico**: PDF directo, DOCX, XLSX, CSV→PDF, texto plano y DOC/XLS legacy).

Decisión de alcance:
- **Visor real de PDF mediante Syncfusion `SfPdfViewer`** (paquetes `Syncfusion.Maui.PdfViewer` 34.2.9 + `Syncfusion.Licensing` 34.2.9). Sustituye al visor propio con `#if ANDROID`/`#if WINDOWS` (`Android.Graphics.Pdf.PdfRenderer` + `Windows.Data.Pdf`) que se descartó por decisión del usuario tras probarla en emulador (sept 2026): no se comportaba como un visor real (scroll discreto por página rasterizada, sin búsqueda ni selección de texto).
- **Licencia**: componente comercial; aplica la **Community License** gratuita (empresas y personas: organizaciones <US$1M de ingresos anuales, ≤5 desarrolladores, ≤10 empleados). La clave se **inyecta en build como `AssemblyMetadata`** (`MauiProgram.cs` lee el atributo y llama `SyncfusionLicenseProvider.RegisterLicense` solo si trae valor): se obtiene de la variable de entorno `SYNC_FUSION_LICENSE_KEY` o de la property MSBuild `-p:SyncfusionLicenseKey=...` (definida en el csproj con fallback a vacío). **La clave nunca se hardcodea ni se versiona** en el repositorio. Sin clave configurada el proyecto compila igual (sin registro; el visor mostraría advertencia trial en runtime). El `Syncfusion.Maui.Toolkit` 1.0.11 ya presente es un producto distinto (free) y coexiste sin conflicto.
- El control cubre las 4 TFMs del csproj: Android, iOS, MacCatalyst y Windows (net10). Build verificado en Android y Windows. **iOS y MacCatalyst no son objetivo del proyecto**: no se compilan ni se prueban porque no hay Mac disponible, y su estado es de mejor esfuerzo (código presente por el control de Syncfusion, sin verificación).

Entregado:
- **Router multipropósito (botón único; Fases 1-3)**: `MenuViewModel.NavigateToPdfReader` usa `IFilePickerService.PickDocumentAsync` (PDF/DOCX/XLSX/DOC/XLS/CSV/texto) y detecta el **tipo real por contenido** (`DocumentTypeDetector`: firma `%PDF-` → PDF; entradas ZIP canónicas `word/document.xml` vs `xl/workbook.xml` → DOCX/XLSX; firma OLE `D0CF11E0` → contenedor legacy, resuelto **por extensión** `.doc`/`.xls` → DOC/XLS (Fase 3, límite documentado: renombrados no se detectan); si no hay firma, lee muestra UTF-8 y decide **CSV** si ≥90% de las líneas comparten el mismo conteo de delimitadores `,`, `;` o tab (`GetBestCsvDelimiter`) o **Texto plano** en caso contrario; presencia de byte de control → `Unknown`). El router enruta: **PDF** → se pasa el `path`; **DOCX/DOC/XLSX/XLS/CSV** → `IDocumentPdfConverter.ConvertToPdfAsync` (DocIO `FormatType.Docx`/`Doc`, XlsIO; para CSV `DocumentTypeDetector.DetectDelimiter(path)` detecta el separador real que se pasa a `Workbooks.Open(path, delimitador)`) y se pasa un `MemoryStream`; **Texto plano** → `PushAsync("TextReaderPage", path)` con el path como parámetro; formato no soportado (`Ole`, `Unknown`) → se ignora; error de conversión → alert localizado (`PdfReaderOpenErrorText` + `CommonOkText`). Navegación PDF mediante objeto `PdfReaderPayload` (`Path` o `Stream` + `FileName`). El botón usa el icono `icon_file.png`.
- Core: `PdfReaderViewModel` — overloads `Load(path)` y `Load(Stream, fileName)`; `PdfDocumentStream`, `FileName`, `HintText`, `IsFileLoaded`; `Unload()` libera el stream y resetea el estado. `TextReaderViewModel` (Fase 2) — visor de texto plano: `Content`, `FileName`, `IsFileLoaded`, `Message`; `Load(path)` lee con StreamReader UTF-8 (hasta 2 MB por `MaxBytesToRead`), `Unload()` resetea el estado; error → `TextReaderOpenErrorText` localizado vía `ILanguageService`.
- MAUI: `PdfReaderPage.xaml` con `<syncfusion:SfPdfViewer>` (`DocumentSource="{Binding PdfDocumentStream}"`; el control aporta toolbar, navegación, zoom, búsqueda y selección de texto) + Label de hint cuando no hay documento; `PdfReaderPage.xaml.cs` resuelve el VM en `OnNavigatedTo`, lee `PdfReaderPayload`, y en **`OnDisappearing`** llama `PdfViewer.UnloadDocument()` + `viewModel.Unload()` para liberar memoria del documento. `TextReaderPage.xaml` (Fase 2): `<Editor>` de solo lectura con `FontFamily="Courier New"` y mensaje de error visible cuando `IsFileLoaded=false`; `TextReaderPage.xaml.cs` resuelve el VM en `OnNavigatedTo` con el path como parámetro (`INavigationService.TakeNavigationParameter()` devuelve `string`), `OnDisappearing` → `Unload()`. `DocumentPdfConverter` y `TextReaderPage`/`TextReaderViewModel` registrados en DI. **Indicador de conversión**: `MenuViewModel.IsConverting` (observable) activa un overlay a pantalla completa en `MenuPage` con `ActivityIndicator` (color `MyAccentBlue` para visibilidad en tema claro/oscuro) + texto `PdfReaderConvertingText` ("Convirtiendo a PDF...") durante DOCX/XLSX/CSV; solo PDF y texto directos no lo activan. resx ×3 (claves `PdfReader*` y `TextReaderOpenErrorText`; se eliminaron `PdfReaderEmptyText` y `PdfReaderPageCountText` por quedar sin uso).
- Tests: `PdfReaderViewModelTests` (4), `DocumentTypeDetectorTests` (18: firma PDF, DOCX/XLSX por entrada ZIP canónica, `Unknown` para ZIP sin entrada esperada/bytes inválidos/stream vacío, OLE→`Ole` + por extensión `.doc`/`.xls`/otra, CSV coma/punto-y-coma/tab, texto de una línea→Texto, texto plano y código→Text, preserva posición, lectura por path, `DetectDelimiter` coma/punto-y-coma/tab/default), `MenuViewModelTests` con router (PDF directo, DOCX convertido, CSV convertido, DOC/XLS convertido, texto→`TextReaderPage`, cancelación del picker, `IsConverting` activo durante la conversión y reseteado en éxito/error) → suite total 214.

Límites conocidos (no resueltos a propósito):
- Sin clave de licencia válida, Syncfusion puede mostrar advertencia de licencia trial en runtime.
- Documentos muy grandes: carga y memoria las maneja el control; validar comportamiento en emulador/dispositivo.
- iOS/MacCatalyst: fuera del alcance del proyecto (sin Mac para compilar/probar); el código queda tal cual por el control de Syncfusion, sin verificación de build ni runtime.
- El visor se unload automáticamente al salir de la página (`OnDisappearing`): al volver hay que volver a abrir el archivo.
- Visor de texto plano (Fase 2): sin resaltado de sintaxis; archivos >2 MB se rechazan con `TextReaderOpenErrorText`; CSV también es convertible a PDF, por lo que la detección prioriza CSV cuando hay alineación de columnas (texto libre de datos con comas puede clasificarse como CSV).
- DOC/XLS legacy (Fase 3): la distinción DOC vs XLS usa la **extensión** del archivo una vez confirmado el contenedor OLE por contenido; un `.doc` o `.xls` mal nombrado (extensión distinta) no se detectaría. La conversión a PDF es la misma que DOCX/XLSX (DocIO `FormatType.Doc` / XlsIO).

#### Problemas detectados en runtime (emulador Android, sept 2026)

- **PDF en blanco al volver a la página**: con las páginas registradas como **Singleton**, salir a cargar otro documento (`OnDisappearing` → `PdfViewer.UnloadDocument()` + `viewModel.Unload()`) y volver/reabrir dejaba el visor en blanco. Referencia: Syncfusion Feedback #59237 / Foro de Syncfusion #189392 (reutilizar una instancia de `SfPdfViewer` tras `UnloadDocument` no soporta cargar documentos posteriores). **Fix aplicado (build 0 errores)**: `PdfReaderPage` ahora es **Transient** (página y control `SfPdfViewer` nuevos por navegación; el VM ya era Transient y se resuelve en `OnNavigatedTo`). **Verificado en runtime**: el flujo abrir PDF → menú → reabrir PDF funciona correctamente.
- **Cancelación de la carga del PDF sin peligro**: verificado en runtime. No es un bug: el visor PDF funciona correctamente. El "cuelgue al cancelar el picker" observado antes en el emulador se debía a que el **emulador no tiene botón "volver"** para cancelar la carga; en **dispositivos físicos el botón volver del sistema cancela correctamente** el picker. Descartada la hipótesis de bug de MAUI (dotnet/maui #33706) y el fix propuesto de timeout en `FilePickerService`.

- BUILD REAL: 0 errores / 0 advertencias; TEST REAL: 214/214 verdes.
## 19. GUÍA de reconstrucción (build desde cero)

Repositorio real: D:\Repo\.NET\NavajaSuiza_.NET10 (sin tildes; git rev-parse y Test-Path OK - verificado §11.6-1).
Convenciones de archivos:
- Markdown planos en UTF-8. No usar oldString byte-exacto en líneas con acentos/CRLF (§11.5);
  preferir anexar secciones al final o verificar por el canal git (§10.3).
- Git: commits los hace el usuario (§11.2). No tocar Git, .env ni dependencias sin autorización (§10-14).

Prerrequisitos:
- .NET 10 SDK con workload MAUI: dotnet workload install maui-android maui-windows
- VS Community 2026 con cargas de trabajo .NET MAUI (Android + Windows)
- Android SDK (API 35+ / JDK 17+) administrado por VS.

Restaurar:
  dotnet restore NavajaSuiza_.NET10/NavajaSuiza_.NET10.csproj

Build (Android Debug):
  dotnet build NavajaSuiza_.NET10/NavajaSuiza_.NET10.csproj -f net10.0-android -c Debug

Suite de tests (esperado: 214 superados / 0 fallos):
  dotnet test NavajaSuiza.Test/NavajaSuiza.Test.csproj
Si la suite falla con "No se pudieron cargar las extensiones" o con el error 0x800711C7, no es un fallo del proyecto: Smart App Control de Windows bloquea el archivo `xunit.runner.visualstudio.testadapter.dll` porque ese paquete viene sin firma digital. Reiniciar el IDE o el equipo lo resuelve; no hace falta cambiar ningún paquete.

Release Android (APK): el default del csproj es AAB (para Google Play). Para generar APK de prueba:
  dotnet clean NavajaSuiza_.NET10/NavajaSuiza_.NET10.csproj -f net10.0-android -c Release
  dotnet build NavajaSuiza_.NET10/NavajaSuiza_.NET10.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
  Ante "Error de proceso de archivado" (lista de errores vacía): limpiar obj/bin y reconstruir;
  no conservar builds previos con encoding dañado (§24).
Licencia Syncfusion: se inyecta con la variable de entorno SYNC_FUSION_LICENSE_KEY antes del build
  (detalles en §18.4); nunca va hardcodeada ni versionada.

Release Windows:
  dotnet build NavajaSuiza_.NET10/NavajaSuiza_.NET10.csproj -f net10.0-windows10.0.19041.0 -c Release

Prueba en dispositivos físicos: instalar APK/MSIX, validar Visor de PDF (§18.4) y herramientas conservadas.
Cuidado §16: no eliminar datos compartidos; aislamiento en tests.

Nota de estado (§15.2): la sección §18.4 quedó deduplicada en esta fase (una sola ocurrencia; antes figuraba duplicada 4 veces).
