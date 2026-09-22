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
| UI Components | Syncfusion.Maui.Toolkit | 1.0.10 |
| Logging | Microsoft.Extensions.Logging.Debug | 10.0.11 |
| Controls | Microsoft.Maui.Controls | 10.0.100 |

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
| **C: Clean Architecture** | Domain + Application + Infrastructure + UI | Extremadamente mantenible | Over-engineering para工具as independientes | Descartada |

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
│   ├── Interfaces/                   # 14 interfaces
│   │   ├── ILanguageService.cs
│   │   ├── IThemeService.cs
│   │   ├── IDeviceStatusService.cs
│   │   ├── INavigationService.cs
│   │   ├── IInstrumentAudioService.cs
│   │   ├── IMetronomeService.cs
│   │   ├── ICompassService.cs
│   │   ├── IOrientationService.cs
│   │   ├── IFlashlightService.cs
│   │   ├── IDeviceDisplayService.cs
│   │   ├── IImagePickerService.cs
│   │   ├── IScreenBrightnessService.cs
│   │   ├── IFlashlightStateService.cs
│   │   └── IStopwatchService.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   └── InstrumentStringData.cs
│   ├── Services/
│   │   ├── FlashlightStateService.cs   # Singleton: persiste estado flash entre VM recreations
│   │   └── StopwatchService.cs         # Singleton: cronómetro (Stopwatch + PeriodicTimer), thread-safe
│   ├── AppConstants.cs
│   └── ViewModels/                     # 19 ViewModels (todas testables, sin dependencias MAUI)
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
│       └── InstrumentViolinViewModel.cs
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
│   │   └── Implementations/           # 12 implementaciones (solo las que usan APIs de plataforma)
│   │       ├── LanguageService.cs
│   │       ├── ThemeService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── NavigationService.cs
│   │       ├── InstrumentAudioService.cs
│   │       ├── MetronomeService.cs
│   │       ├── CompassSensorService.cs
│   │       ├── OrientationSensorService.cs
│   │       ├── FlashlightService.cs
│   │       ├── DeviceDisplayService.cs
│   │       ├── ImagePickerService.cs
│   │       └── ScreenBrightnessService.cs
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
    └── *Tests.cs                     # xUnit + Moq, 193 tests
```

#### Regla de separación Core vs MAUI

| Va a Core (reutilizable, net10.0 puro) | Se queda en MAUI (depende de APIs de plataforma) |
|----------------------------------------|--------------------------------------------------|
| `BaseViewModel` | `NavigationService` (usa `Shell.Current`) |
| Todos los ViewModels (19) | `LanguageService` (usa `Preferences`, `CultureInfo`) |
| `ICompassService`, `IOrientationService` | `ThemeService` (usa `Application.Current`, Android Window) |
| `IFlashlightService`, `IFlashlightStateService` | `DeviceStatusService` (usa `Battery.Default`, Android APIs) |
| `IDeviceDisplayService`, `IImagePickerService` | `CompassSensorService` (usa `Compass.Default`, `OrientationSensor`) |
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
| `IDeviceStatusService` | `DeviceStatusService` | Singleton | Nivel de batería y almacenamiento disponible |
| `INavigationService` | `NavigationService` | Transient | Navegación Shell (`PushAsync`, `GoToAsync`, `DisplayAlertAsync`), null-safe |
| `ICompassService` | `CompassSensorService` | Singleton | Lectura de brújula (`Compass.Default`) |
| `IOrientationService` | `OrientationSensorService` | Singleton | Lectura de orientación (`OrientationSensor.Default`) |
| `IFlashlightService` | `FlashlightService` | Singleton | Control de flash (`Flashlight.Default`) |
| `IDeviceDisplayService` | `DeviceDisplayService` | Singleton | Control de brillo y `KeepScreenOn` |
| `IImagePickerService` | `ImagePickerService` | Singleton | Selección de imagen (`FilePicker`) |
| `IFilePickerService` | `FilePickerService` | Singleton | Selección de archivos (TXT/CSV/DOCX/XLSX) |
&lt;!-- DocumentPdfConverter eliminado --&gt;
| `IScreenBrightnessService` | `ScreenBrightnessService` | Singleton | Brillo de pantalla nativo (Android) |
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
- Encender/apagar linterna del dispositivo (`Flashlight.Default`).
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

### 6.6 Encuadre de imagen (`FramingPage`)
- Relaciones de aspecto: 1:1, 4:5, 9:16, 16:9.
- Modo de ajuste: AspectFill.
- Fondos: Blur, colores sólidos.
- Carga de imagen desde galería (`FilePicker`).

### 6.7 Manual (`ManualPage`)
- Página informativa estática.

### 6.8 Acerca de (`AboutPage`)
- Toggle de tema oscuro/claro con persistencia. En el **primer arranque** se aplica y persiste el tema oscuro (`ThemeService.ApplySavedTheme()` llama `SaveThemePreference(true)` cuando no hay preferencia guardada).
- **Versión** leída en runtime vía `IAppInfoService` (`AppInfo.Current`); única fuente de verdad: el `.csproj` (`ApplicationDisplayVersion`/`ApplicationVersion`). No duplicar versión en constantes.
- **Enlaces**: URLs centralizadas en `AppConstants.About` (Core) y abiertas con `ILauncherService` (`Launcher.Default`). El botón de donación solo se muestra si `DonationUrl` está configurada; el botón de repositorio fue eliminado.
- **Sitio web**: línea `© 2026 | francisco-dev.cl` completa como hipervínculo (un solo label con `TapGestureRecognizer`) hacia `AppConstants.About.WebsiteUrl`, con el color de texto del sistema (compatible tema claro/oscuro), igual que el label de versión (sin subrayado ni opacidad).
- **Versión 3 partes**: `ApplicationDisplayVersion=1.0.1` (texto libre válido en Android/iOS) y `ApplicationVersion=3` como build interno.

### 6.9 Cronómetro (`StopwatchPage`)
- Iniciar/pausar/reiniciar con display `HH:mm:ss.mmm` (3 decimales) y **registro de marcas (vueltas)**.
- **Botones fijos** (no intercambian icono): `Play` (arranca; si ya corre, agrega una marca a la lista sin detener) y `Stop` (detiene; oprimido de nuevo limpia el cronómetro y la lista, dejando `00:00:00.000`).
- **Marcas**: `StopwatchLap` (Number, Split, Delta) en `ObservableCollection`; cada marca guarda el tiempo acumulado y la diferencia con la anterior, mostradas en una lista. Las marcas viven en `StopwatchService` (Singleton) y el VM las refleja en `Initialize()`.
- **Servicio Singleton en Core**: `StopwatchService` usa `System.Diagnostics.Stopwatch` + `PeriodicTimer` (intervalo 10 ms), thread-safe con lock. La UI se actualiza por evento `Tick` con `TimeSpan`.
- **Detención instantánea**: el ticker verifica cancelación antes de cada emisión y `Stop()` cancela antes de resetear, evitando que un tick residual "siga contando" tras pausar/reiniciar.
- **Marcas persistentes**: viven en el servicio Singleton (no en el VM Transient), por lo que persisten al salir y volver a la página y se limpian únicamente con el reset (segundo toque de Stop).
- **Estado persistente entre navegaciones**: al salir de la página el conteo continúa (patrón `FlashlightStateService`); `StopwatchViewModel.Initialize()` resincroniza al volver.
- **Iconos**: botones fijos `icon_play.png` (Play/Marca) y `icon_stop.png` (Stop/Reset), sin `DataTrigger`. El asset `icon_stopwatch.png` se usa como icono del ítem del cronómetro en `MenuPage`.
- **Nota de threading**: el `Tick` se dispara desde hilo background; validado en dispositivo, .NET MAUI refleja correctamente los cambios de propiedades bindables en la UI.

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
| `BoolToLocalizedStringConverter` | Convierte `bool` a string localizado (TrueResourceKey/FalseResourceKey) |
| `BoolToColorConverter` | Convierte `bool` a color (TrueColor/FalseColor). Soporta `FalseColorLight`/`FalseColorDark` para theme-aware |
| `InvertedBoolConverter` | Invierte `bool` (true→false, false→true). Usado para `!IsBusy` en bindings de IsEnabled |

---

## 11. Assets de audio

37 archivos WAV en `Resources/Raw/`:

| Prefijo | Instrumento | Cantidad |
|---------|-------------|----------|
| `GN_` | Guitarra Nylon | 6 |
| `GS_` | Guitarra Acero | 6 |
| `B_` | Bajo | 5 |
| `U_` | Ukulele | 4 |
| `V_` | Violín | 4 |
| `C_` | Charango | 6 |
| `tik_` | Metrónomo | 2 |

---

## 12. Plataformas y permisos

### Permisos Android (`AndroidManifest`)
- `CAMERA`
- `FLASHLIGHT`

**`INTERNET`**: lo re-inyecta `CommunityToolkit.Maui` en el manifest fusionado (`obj/.../AndroidManifest.xml`); permiso *normal*, se conserva.

**No declarados** (principio de mínimo privilegio, eliminados): `BATTERY_STATS` (protegido, no se usa; ver batería abajo), `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`, `ACCESS_NETWORK_STATE`, `INTERNET` (en el manifest fuente).

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
| 33 | Reducir permisos Android a mínimo (quitar `BATTERY_STATS`, location, network, `INTERNET` del source) | `Platforms/Android/AndroidManifest.xml` | ✅ Completado (manifest fusionado verificado) |
| 34 | Empaquetado multirarquitectura (RIDs arm/arm64/x64; A11 32-bit compatible) | `NavajaSuiza_.NET10.csproj` | ✅ Completado (APK Release fat 57,9 MB, 3 ABIs) |
| 35 | Batería sin `BATTERY_STATS` vía `BatteryManager` (Android) | `DeviceStatusService.cs` | ✅ Completado |
| 36 | Cronómetro: marcas persisten entre navegaciones (servicio Singleton) | Core (`Stopwatch*`) + tests | ✅ Completado (144 tests) |

**Pendiente de release**: Privacy Policy **publicada** en `https://www.francisco-dev.cl/navaja-suiza/privacy-policy` (fuente en `PRIVACY_POLICY.md`, trilingüe + bloque Astro) y **`allowBackup=false` decidido** (Notas solo locales, sin transmisión). Resta: pegar la URL en el listing de Play Console, completar el formulario Data Safety, validar target SDK del AAB (targetSdk 36 cumple), assets de tienda (icono adaptativo 512, splash, screenshots, listing trilingüe ES/EN/SV), Release AAB firmado + internal/closed testing → producción.

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

21. **Batería no visible tras la limpieza de permisos**: MAUI `Battery.Default` exige `BATTERY_STATS` en Android. Solución: lectura con `BatteryManager.GetIntProperty(BatteryProperty.Capacity)` (sin permiso). `BATTERY_STATS` NO se re-agrega (permiso protegido, red flag en Play).

22. **Cronómetro: las marcas se perdían al navegar**: `Laps` vivía en el ViewModel (Transient). Solución: persistencia en `StopwatchService` (Singleton); las marcas se limpian únicamente con el reset (segundo toque de Stop).

### 15.2 Issues pendientes (Backlog)

- **Metrónomo — audio de baja latencia**: el clic usa `MediaElement` + `PeriodicTimer` con salto al UI thread (jitter y deriva acumulada). Plan: refactor a servicio `MetronomeClickService` por plataforma (Android `SoundPool`, iOS `AudioToolbox.SystemSound`) + scheduler con tiempos absolutos (`Stopwatch`) para eliminar deriva. **Sin dependencias nuevas.** Referencia: jfversluis/Plugin.Maui.Audio#89 documenta latencia de 150-200 ms incluso con player precargado.
- **Weather — módulo del clima**: evaluar Open-Meteo (gratis, sin API key) cuando se implemente.
- **Biblioteca de componentes MAUI**: la planificación se extrae a un proyecto independiente (no entra en el alcance de esta app). El documento de planificación se movió fuera del repositorio.
- **FilePicker Android — cuelgue al cancelar la selección (Lector/PDF)**: ver §18.4. Bug de MAUI (dotnet/maui #33706; fix PR #33888 no presente en MAUI 10.0.100). Fix propuesto: guarda de timeout en `FilePickerService` (`Task.WhenAny`, sin dependencias nuevas). Estado: hipótesis sin confirmar, sin implementar.
- **Lector — XLSX falla en runtime Android (emulador)**: la conversión XLSX→PDF falla en Android (docx sí funciona). El test de conversión pasa en Windows. Pendiente capturar la excepción real en logcat; hipótesis: limitación de plataforma de `XlsIORenderer` en Android. Posibles caminos si se confirma: limitar el picker a DOCX en Android, o fallback a extracción de valores + render propio de grid.
- **Deploy Android automatizado falla ("No se pudo obtener el id. de proceso para 'com.nefelin.navajasuiza'")**: la app se instala y arranca bien manualmente (`adb shell am start`), pero el lanzador/depurador expira esperando el pid (arranque en frío ~6 s + Fast Deployment; log `monodroid-debug: Not starting the debugger as the timeout value has been reached`). Operativo: `adb` **no está en PATH** (ruta `C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe`); si el Run falla, probar `adb uninstall com.nefelin.navajasuiza` y reintentar, y si persiste reiniciar el emulador.

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

Pasos 1–2 completados; 3–6 según el plan §14 Fase F.

---

## 18. Features planadas (backlog de desarrollo)

### 18.1 Pizarra (dibujo)

Estado: **Fases 1 y 2 implementadas y verificadas** (170 tests; builds Android/Windows 0 errores). Fase 2 = export WebP a galería, ahora **transversal vía SkiaSharp** (verificado en emulador Android y en Windows). **Goma descartada**: Deshacer (LIFO) + Limpiar cubren el caso de esta app.

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

Pendiente (Fase 3):
- **Compartir** la imagen exportada desde la app (Share API de MAUI).
- **iOS**: guardado en Photos (requeriría `NSPhotoLibraryAddUsageDescription` en el Info.plist).
- La **goma quedó descartada** (Deshacer/ Limpiar cubren el caso; Deshacer es LIFO y Limpiar total — ver estado).

Enfoque técnico de export (referencia):
- **MAUI no exporta `GraphicsView` a archivo**: se re-rasterizan los trazos desde el modelo. Ahora con **SkiaSharp** (una dependencia, raster + WebP transversal para todas las plataformas). Se evaluó un rasterizador propio en Core + encoder PNG (0 dependencias) pero se descartó: más líneas de gráficas que mantener y se perdía el WebP liviano.
- **`MediaSaver`/`FileSaver` de CommunityToolkit no existen en v15.0.1** (verificado en el cache del paquete), por lo que la escritura a galería usa `MediaStore` nativo (solo esa parte es de plataforma).
- SkiaSharp habilita además: export a imagen de futuros dibujos, herramientas de figuras/texto en la Pizarra y gráficos propios (ej. historial del Contador de pasos).

### 18.2 Contador de pasos

Estado: **planificada** — enfoque definido; pendiente confirmar alcance y autorización para implementar.

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

### 18.3 Lector de archivos (TXT/CSV/DOCX/XLSX) — ELIMINADO

Estado: **eliminada por decisión de alcance (2026)**. La feature "Lector de archivos/documentos" (TXT/CSV/DOCX/XLSX) fue retirada del proyecto: se eliminaron `DocumentReaderViewModel`, `DocumentReaderPage`, `IDocumentPdfConverter`/`DocumentPdfConverter` (DOCX/XLSX→PDF), `FilePickerService` con tipos adicionales y los tests `DocumentReaderViewModelTests`/`DocumentPdfConverterTests`. **Se conservó únicamente el visor de PDF** (§18.4) y su picker de `.pdf`. Tras el retiro, la suite queda en **192** tests.

Decisión de alcance:
- Lectura con `FilePicker` de **TXT** (texto plano), **CSV** (grid) y **DOCX/XLSX** (convertidos a PDF y mostrados en `SfPdfViewer`, §18.4). La fase XLSX/DOCX original se había eliminado porque el extractor perdía el layout; se **reintrodujo cambiando el enfoque**: en lugar de extraer texto, el documento se **convierte a PDF** con Syncfusion (`DocIORenderer.NET`/`XlsIORenderer.NET`) y se renderiza con el visor real, preservando el layout.
- CSV se muestra como **tabla** (`SfDataGrid`, `Syncfusion.Maui.DataGrid` 34.2.8) construida desde un `DataTable`. **Encabezado**: con ≥2 filas la primera es encabezado aunque las filas sean irregulares; las columnas toman el nombre del header y las sobrantes "Columna N"; las filas más cortas se rellenan con celdas vacías.

Entregado:
- Core: `IFilePickerService` (ruta `string?`, tipos por extensión), `TextFileDecoder` (BOM UTF-8/UTF-16LE/UTF-16BE, UTF-8 estricto, fallback **Latin-1** sin dependencias), `CsvParser` (RFC-ish: comillas, comas/saltos de línea dentro de comillas, `""` escapado, CRLF/LF, filas vacías omitidas), `DocumentReaderViewModel` (decodificar/parsear/convertir por extensión; `DataTable` para CSV; `PdfDocumentStream` para DOCX/XLSX) e **`IDocumentPdfConverter`/`DocumentPdfConverter`** (DOCX/XLSX → PDF en `Task.Run`, devuelve `MemoryStream`).
- MAUI: `FilePickerService` con MIME `.txt/.csv/.docx/.xlsx` por plataforma, `DocumentReaderPage` con **3 vistas según tipo** (Editor para texto, `SfDataGrid` para CSV, `SfPdfViewer` para PDF/DOCX/XLSX), **`OnDisappearing`** → `PdfViewer.UnloadDocument()` + `viewModel.Unload()`, DI (**página Transient** — ver §18.4, problema del visor en blanco; VM Transient; `FilePickerService` y `DocumentPdfConverter` Singleton), ítem de menú `icon_documents.png` y resx ×3 actualizados.
- Tests: `CsvParserTests` (10) y `TextFileDecoderTests` (7) → conservados. `DocumentReaderViewModelTests` y `DocumentPdfConverterTests` fueron **eliminados junto con la feature** (§18.3) → suite total **192**.

Pendientes (ver §15.2):
- **XLSX en Android/emulador**: la conversión falla en runtime (docx sí funciona). El test de conversión XLSX pasa en Windows; hipótesis pendiente de confirmar con la excepción real (logcat): limitación de plataforma de `XlsIORenderer` en Android.
- Verificación runtime restante: Windows (docx/xlsx) y flujo PDF completo.

### 18.4 Visor de PDF (Syncfusion SfPdfViewer)

Estado: **implementada y verificada a nivel de build/tests** (Android/Windows 0/0; tests del suite en total 204). Verificación runtime pendiente en dispositivo/emulador.

Decisión de alcance:
- **Visor real de PDF mediante Syncfusion `SfPdfViewer`** (paquetes `Syncfusion.Maui.PdfViewer` 34.2.8 + `Syncfusion.Licensing` 34.2.8). Sustituye al visor propio con `#if ANDROID`/`#if WINDOWS` (`Android.Graphics.Pdf.PdfRenderer` + `Windows.Data.Pdf`) que se implementó antes y luego se **descartó por decisión del usuario tras probarla en emulador (sept 2026)**: no se comportaba como un visor real (scroll discreto por página rasterizada, sin búsqueda ni selección de texto).
- **Licencia**: componente comercial; aplica la **Community License** gratuita (empresas y personas: organizaciones <US$1M de ingresos anuales, ≤5 desarrolladores, ≤10 empleados). Requiere `SyncfusionLicenseProvider.RegisterLicense(clave)` con la clave comunitaria que se obtiene en syncfusion.com; la clave se registra en `MauiProgram.cs` (gestionarla con cuidado: mantenerla fuera de repositorios públicos/logs). El `Syncfusion.Maui.Toolkit` 1.0.11 ya presente es un producto distinto (free) y coexiste sin conflicto.
- El control cubre las 4 TFMs del csproj: Android, iOS, MacCatalyst y Windows (net10); build verificado en Android y Windows; iOS/MacCatalyst pendientes (requieren Mac).

Entregado:
- Core: `PdfReaderViewModel` simplificado — `PdfDocumentStream` (FileStream del archivo elegido), `FileName`, `HintText`, `IsFileLoaded`; `OpenDocumentCommand` → `IFilePickerService.PickPdfAsync` + apertura del stream; `Unload()` libera el stream y resetea el estado. Se eliminaron del pipeline anterior: `IPdfRendererService`, `PdfRendererService`, `PdfPageItem`, batching/`RenderPixelWidth` y los gestos de zoom propios.
- MAUI: `PdfReaderPage.xaml` con `<syncfusion:SfPdfViewer>` (`DocumentSource="{Binding PdfDocumentStream}"`; el control aporta toolbar, navegación, zoom, búsqueda y selección de texto) + Label de hint cuando no hay documento; `PdfReaderPage.xaml.cs` resuelve el VM en `OnNavigatedTo` y en **`OnDisappearing`** llama `PdfViewer.UnloadDocument()` + `viewModel.Unload()` para liberar memoria del documento. `MauiProgram.cs`: `ConfigureSyncfusionCore()` + `RegisterLicense`; se quitó el DI del servicio de render. resx ×3 (4 claves `PdfReader*`; se eliminaron `PdfReaderEmptyText` y `PdfReaderPageCountText` por quedar sin uso).
- Tests: `PdfReaderViewModelTests` (4: carga OK con archivo temporal, cancelación del picker, error al abrir, `Unload`) → suite total 204.

Límites conocidos (no resueltos a propósito):
- Sin clave de licencia válida, Syncfusion puede mostrar advertencia de licencia trial en runtime.
- Documentos muy grandes: carga y memoria las maneja el control; validar comportamiento en emulador/dispositivo.
- iOS/MacCatalyst: implementación presente por el control, pero compilación no verificada (requiere Mac).
- El visor se unload automáticamente al salir de la página (`OnDisappearing`): al volver hay que volver a abrir el archivo.

#### Problemas detectados en runtime (emulador Android, sept 2026)

- **PDF en blanco al volver a la página**: con las páginas registradas como **Singleton**, salir a cargar otro documento (`OnDisappearing` → `PdfViewer.UnloadDocument()` + `viewModel.Unload()`) y volver/reabrir dejaba el visor en blanco. Referencia: Syncfusion Feedback #59237 / Foro de Syncfusion #189392 (reutilizar una instancia de `SfPdfViewer` tras `UnloadDocument` no soporta cargar documentos posteriores). **Fix aplicado (build 0 errores)**: `DocumentReaderPage` y `PdfReaderPage` ahora son **Transient** (página y control `SfPdfViewer` nuevos por navegación; los VMs ya eran Transient y se resuelven en `OnNavigatedTo`). **Verificación runtime pendiente** (flujo: abrir PDF → menú → reabrir PDF).
- **App congelada al cancelar el picker (Lector/Visor de PDF)**: al pulsar "cargar archivo" y cerrar el picker sin elegir, la app queda sin respuesta y hay que matar el proceso. Hipótesis principal: bug de MAUI en Android — el `IntermediateActivity` del picker se destruye sin `OnActivityResult` cuando la `MainActivity` se recrea mientras el picker está abierto, dejando el `TaskCompletionSource` de `FilePicker.PickAsync()` sin resolver para siempre (dotnet/maui #33706; fix en PR #33888, **no incluido en MAUI 10.0.100**). El emulador (arranque en frío ~6 s, poca memoria) favorece esa recreación. **Fix propuesto (sin dependencias nuevas)**: guarda con `Task.WhenAny` + timeout (~20 s) + captura de cancelación en `FilePickerService.PickDocumentAsync`/`PickPdfAsync`. **Pendiente**: confirmar la reproducción vía logcat y autorización para implementarlo (ver backlog §15.2).

### 18.4 Visor de PDF (rediseno 2026)

- `MenuViewModel` ya no navega directamente: `NavigateToPdfReader` usa `IFilePickerService.PickPdfAsync` (titulo via `ILanguageService.GetString`) y solo navega a `PdfReaderPage` con el path como parametro si el usuario eligio un archivo.
- `PdfReaderViewModel` no depende del picker: expone `Load(string path)` / `Unload()`, propiedades `PdfDocumentStream`, `FileName`, `IsFileLoaded`. Ctor: `(ILogger<PdfReaderViewModel>, INavigationService)`.
- `PdfReaderPage` es full-screen (sin boton ni hint): en `OnNavigatedTo` toma `TakeNavigationParameter()` y llama a `Load`.
- IDioma del picker: `PdfReaderPickerTitleText` (Visor/Viewer/Visare).
### 18.4 Visor de PDF (PdfReader)
- Reescrito: MenuViewModel usa IFilePickerService para elegir el archivo (titulo localizado via ILanguageService) y navega a PdfReaderPage con el path como parametro.
- PdfReaderViewModel expone Load(path) / Unload() y propiedades PdfDocumentStream, FileName, IsFileLoaded. Ctor: ILogger + INavigationService.
- PdfReaderPage es full-screen (sin boton ni hint); en OnNavigatedTo lee el parametro via TakeNavigationParameter() y llama a Load.
- Tests: MenuViewModelTests y PdfReaderViewModelTests cubren picker, Load/Unload y estado.

### 18.4 Visor de PDF (rediseno)
- MenuViewModel usa IFilePickerService (IFilePickerService.PickPdfAsync con titulo de ILanguageService.GetString) y navega a PdfReaderPage con el path como parametro.
- PdfReaderViewModel expone Load(path) / Unload() y propiedades PdfDocumentStream, FileName, IsFileLoaded. Ctor: ILogger + INavigationService (sin picker en el VM).
- PdfReaderPage es full-screen sin boton ni hint: en OnNavigatedTo toma el path de TAKE_NAVIGATION_PARAMETER y llama a Load.
- Tests: PdfReaderViewModelTests cubren Load valido, path inexistente (reset) y Unload (dispose + reset); MenuViewModelTests cubren picker ok y picker cancelado sin navegacion.

- BUILD REAL: 0 errores / 0 advertencias; TEST REAL: 205/205 verdes (13s).
