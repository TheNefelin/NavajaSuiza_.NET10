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
│   ├── Interfaces/                   # 13 interfaces
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
│   │   └── IFlashlightStateService.cs
│   ├── Models/
│   │   ├── SupportedLanguages.cs
│   │   └── InstrumentStringData.cs
│   ├── Services/
│   │   └── FlashlightStateService.cs   # Singleton: persiste estado flash entre VM recreations
│   ├── AppConstants.cs
│   └── ViewModels/                     # 18 ViewModels (todas testables, sin dependencias MAUI)
│       ├── BaseViewModel.cs
│       ├── AboutViewModel.cs
│       ├── MenuViewModel.cs
│       ├── MetronomeViewModel.cs
│       ├── TunerViewModel.cs
│       ├── ManualViewModel.cs
│       ├── TestingViewModel.cs
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
    └── *Tests.cs                     # xUnit + Moq, 62 tests
```

#### Regla de separación Core vs MAUI

| Va a Core (reutilizable, net10.0 puro) | Se queda en MAUI (depende de APIs de plataforma) |
|----------------------------------------|--------------------------------------------------|
| `BaseViewModel` | `NavigationService` (usa `Shell.Current`) |
| Todos los ViewModels (18) | `LanguageService` (usa `Preferences`, `CultureInfo`) |
| `ICompassService`, `IOrientationService` | `ThemeService` (usa `Application.Current`, Android Window) |
| `IFlashlightService`, `IFlashlightStateService` | `DeviceStatusService` (usa `Battery.Default`, Android APIs) |
| `IDeviceDisplayService`, `IImagePickerService` | `CompassSensorService` (usa `Compass.Default`, `OrientationSensor`) |
| `IScreenBrightnessService` | `OrientationSensorService` (usa `OrientationSensor.Default`) |
| `INavigationService`, `ILanguageService`, `IThemeService` | `FlashlightService` (usa `Flashlight.Default`) |
| `IInstrumentAudioService`, `IMetronomeService` | `DeviceDisplayService` (usa `DeviceDisplay.Current`) |
| `InstrumentStringData`, `SupportedLanguages` | `ImagePickerService` (usa `FilePicker`) |
| `AppConstants` | `ScreenBrightnessService` (usa Android brightness APIs) |
| `FlashlightStateService` (Core.Services) | `InstrumentAudioService` (usa `MediaElement`, `Border`) |
| | `MetronomeService` (usa `MediaElement`) |

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
| `IScreenBrightnessService` | `ScreenBrightnessService` | Singleton | Brillo de pantalla nativo (Android) |
| `IFlashlightStateService` | `FlashlightStateService` (Core) | Singleton | Persistencia de estado flash entre recreaciones de VM, thread-safe con lock |
| `IMetronomeService` | `MetronomeService` | Transient | Metrónomo con `PeriodicTimer` y reproducción de audio |
| `IInstrumentAudioService` | `InstrumentAudioService` | Singleton | Administra audio e interpolación de vibración de instrumentos. Singleton necesario para control centralizado del MediaElement y parada garantizada al cambiar de tab. |

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
| `MetronomeService` | Transient | Timer y estado por instancia |
| `InstrumentAudioService` | Singleton | Control centralizado del MediaElement y audio |
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
- Toggle de tema oscuro/claro con persistencia.

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
- `BATTERY_STATUS`
- `CAMERA`
- `FLASHLIGHT`
- `ACCESS_COARSE_LOCATION`
- `ACCESS_FINE_LOCATION`

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
| Servicios como Singleton | Compartir estado de audio (MediaElement) entre páginas |
| Localización por .resx | Mecanismo nativo .NET, soporte XAML y C#, fallback automático |
| Audio via MediaElement | Componente nativo del CommunityToolkit.Maui con soporte multiplataforma |

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
| 10 | `B_00_B0.wav` no referenciado | `InstrumentAudioService.cs` | Baja | Pendiente |
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
| 25 | Crear proyecto de tests | NavajaSuiza.Test | ✅ Completado (62 tests) |

### Fase D — Enriquecimiento de SKILL.md

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 26 | Agregar sección MAUI completa al SKILL (transversal) | `SKILL.md` | ✅ Completado |

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
18. **Audio de instrumento persiste al cambiar de tab Shell**: `InstrumentAudioService` era Transient y el `MediaElement` del Page Singleton no se detenía correctamente al cambiar de pestaña. Cuando el usuario cambiaba de tab (e.g., Menu → Instrumento → About), el audio continuaba. Causa: el Service Transient guardaba referencia a MediaElement que persistía, pero al cambiar de tab el OnDisappearing no garantizaba el stop inmediato. **Solución**: Cambiar `InstrumentAudioService` a Singleton + llamada explícita a `StopAllStringAsync()` en `OnDisappearing` de cada página de instrumento. Esto asegura que el singleton global administre el MediaElement y detenga el audio al navegar fuera.

### 15.2 Issues pendientes

1. **`TestingPage`/`TestingViewModel` vacíos**: Página de pruebas sin implementación.
2. **AboutPage navigation crash**: `GoToAsync("//AboutPage")` causa `JavaProxyThrowable` en Android. Botón oculto por `IsDevelopment`, no es bug visible. Documentado en `MenuViewModel.cs`.
3. **`B_00_B0.wav` no referenciado**: Archivo de audio sin uso en instrumentos.

---

## 16. Referencias

- **SKILL.md**: Guía transversal de patrones .NET Senior (APIs REST + MAUI).
- **AGENTS.md**: Reglas de operación para OpenCode en este proyecto.
- **README.md**: Documentación general del proyecto (pendiente de actualización).
- **NavajaSuiza.Core**: Class Library con interfaces y modelos compartidos.
