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
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.4.0 |
| UI Toolkit | CommunityToolkit.Maui | 13.0.0 |
| Media | CommunityToolkit.Maui.MediaElement | 6.1.3 |
| UI Components | Syncfusion.Maui.Toolkit | 1.0.8 |
| Gráficos | SkiaSharp.Views.Maui.Controls | 3.119.1 |
| Gráficos Extended | SkiaSharp.Extended | 3.0.0 |
| Logging | Microsoft.Extensions.Logging.Debug | 10.0.1 |
| Controls | Microsoft.Maui.Controls | 10.0.20 |

---

## 3. Arquitectura

### 3.1 Decisión arquitectónica: Core + MAUI

**Fecha de decisión**: 2026-08-25  
**Estado**: Aprobada, pendiente de implementación

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

La aplicación sigue el patrón **MVVM** (Model-View-ViewModel) utilizando el CommunityToolkit.Mvvm:

- **Models**: Modelos de datos simples (`InstrumentStringData`, `SupportedLanguages`).
- **Views** (Pages): Páginas XAML con code-behind mínimo (solo inicialización de BindingContext y ciclos de vida).
- **ViewModels**: Lógica de presentación, comandos y estado de UI. Extienden `BaseViewModel`.
- **Services**: Lógica de negocio y acceso a APIs de plataforma, separada de los ViewModels mediante interfaces.

### 3.3 Estructura actual (pre-refactor)

```
NavajaSuiza_.NET10/
├── Pages/                    # Vistas XAML + code-behind
│   ├── Components/           # Componentes reutilizables (ContentView)
│   └── *.xaml / *.xaml.cs
├── PagesViewModel/           # ViewModels (nombre no estándar)
├── Services/
│   ├── Interfaces/           # Contratos de servicio
│   └── Implementations/      # Implementaciones concretas
├── Models/                   # Modelos de datos
├── Converters/               # Convertidores de valor XAML
├── Extensions/               # Extensiones y utilidades (localización)
├── Behaviors/                # Carpeta creada pero vacía
├── Resources/
│   ├── AppIcon/              # Icono de la aplicación
│   ├── Fonts/                # Fuentes (OpenSans)
│   ├── Images/               # Imágenes SVG y PNG
│   ├── Languages/            # Archivos .resx de localización
│   ├── Raw/                  # Assets de audio (WAV)
│   ├── Splash/               # Pantalla de splash
│   └── Styles/               # Estilos y colores XAML
├── Platforms/                # Código específico por plataforma
├── App.xaml/cs               # Punto de entrada de la aplicación
├── AppShell.xaml/cs          # Shell de navegación
└── MauiProgram.cs            # Configuración de DI y servicios
```

### 3.4 Estructura objetivo (post-refactor)

```
NavajaSuiza.sln
├── NavajaSuiza.Core/                  # Class Library (net10.0)
│   ├── Models/
│   │   ├── InstrumentStringData.cs
│   │   └── SupportedLanguages.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── ILanguageService.cs
│   │   │   ├── IThemeService.cs
│   │   │   ├── IDeviceStatusService.cs
│   │   │   ├── IMetronomeService.cs
│   │   │   ├── IInstrumentAudioService.cs
│   │   │   └── IImageProcessingService.cs
│   │   └── Implementations/
│   │       └── LanguageService.cs     # Solo las que NO dependen de MAUI APIs
│   └── Extensions/
│       └── LocalizationResourceManager.cs
│
├── NavajaSuiza.UI/                    # Proyecto MAUI (net10.0)
│   ├── Pages/
│   │   ├── Components/
│   │   └── *.xaml / *.xaml.cs
│   ├── ViewModels/                    # Renombrado de PagesViewModel
│   │   └── BaseViewModel.cs
│   ├── Services/
│   │   └── Implementations/           # Solo las que dependen de MAUI APIs
│   │       ├── ThemeService.cs
│   │       ├── DeviceStatusService.cs
│   │       ├── MetronomeService.cs
│   │       ├── InstrumentAudioService.cs
│   │       └── ImageProcessingService.cs
│   ├── Converters/
│   ├── Platforms/
│   ├── Resources/
│   ├── App.xaml/cs
│   ├── AppShell.xaml/cs
│   └── MauiProgram.cs
```

#### Regla de separación Core vs MAUI

| Va a Core (reutilizable) | Se queda en MAUI (depende de APIs de plataforma) |
|--------------------------|--------------------------------------------------|
| `InstrumentStringData` | `DeviceStatusService` (usa `Battery.Default`, Android APIs) |
| `SupportedLanguages` | `ThemeService` (usa `Application.Current`, Android Window) |
| `ILanguageService` + `LanguageService` | `ScreenLightViewModel` (usa Android brightness) |
| `IInstrumentAudioService` | `FlashlightViewModel` (usa `Flashlight.Default`) |
| `IMetronomeService` | `CompassViewModel` (usa `Compass.Default`) |
| `IImageProcessingService` | `TranslateExtension` (depende de MAUI XAML) |
| `LocalizationResourceManager` | `InstrumentStringComponent` (ContentView) |
| | `MetronomeService` (usa `MediaElement` — CommunityToolkit.Maui) |

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

Todos los servicios están registrados como **Singleton** en `MauiProgram.cs` e inyectados por DI.

| Interfaz | Implementación | Responsabilidad |
|----------|---------------|-----------------|
| `ILanguageService` | `LanguageService` | Localización, cambio de idioma, persistencia en `Preferences` |
| `IThemeService` | `ThemeService` | Tema oscuro/claro, persistencia, status bar (Android) |
| `IDeviceStatusService` | `DeviceStatusService` | Nivel de batería y almacenamiento disponible |
| `IMetronomeService` | `MetronomeService` | Metrónomo con `System.Timers.Timer` y reproducción de audio |
| `IInstrumentAudioService` | `InstrumentAudioService` | Configuración de cuerdas, reproducción de audio, vibración de UI |
| `IImageProcessingService` | `ImageProcessingService` | Procesamiento de imagen (**parcialmente implementado**) |

### 5.2 Lifetime de servicios

| Servicio | Lifetime recomendado | Justificación |
|----------|---------------------|---------------|
| `LanguageService` | Singleton | Sin estado persistente en memoria, seguro como Singleton |
| `ThemeService` | Singleton | Sin estado mutable |
| `DeviceStatusService` | Singleton | Lee datos en cada llamada, sin estado |
| `MetronomeService` | Singleton | Mantiene referencia a MediaElement y timer; requiere Stop() al salir |
| `InstrumentAudioService` | Singleton | Mantiene `_borderAudioMap` y `_mediaElement`; requiere ClearAllBorders() al salir |
| `ImageProcessingService` | Singleton | Sin estado |

---

## 6. Herramientas (Features)

### 6.1 Linterna (`FlashlightPage`)
- Encender/apagar linterna del dispositivo (`Flashlight.Default`).
- Navegar a pantalla de luz completa (`ScreenLightPage`).
- Manejo de errores con reversión de estado.

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

### 6.6 Encuadre de imagen (`FramingPage`)
- Relaciones de aspecto: 1:1, 4:5, 9:16, 16:9.
- Modo de ajuste: AspectFill.
- Fondos: Blur, colores sólidos.
- Carga de imagen desde galería (`FilePicker`).
- Procesamiento de imagen con SkiaSharp (**pendiente de implementación completa**).

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
| `BoolToColorConverter` | Convierte `bool` a color (TrueColor/FalseColor) |

---

## 11. Assets de audio

37 archivos WAV en `Resources/Raw/`:

| Prefijo | Instrumento | Cantidad |
|---------|-------------|----------|
| `GN_` | Guitarra Nylon | 6 |
| `GS_` | Guitarra Acero | 6 |
| `B_` | Bajo | 5 (incluye B_00_B0.wav no referenciado) |
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
| Core + MAUI (separación) | Lógica reutilizable en Class Library, UI en MAUI. Escalable sin over-engineering |
| Servicios como Singleton | Compartir estado de audio (MediaElement) entre páginas |
| Localización por .resx | Mecanismo nativo .NET, soporte XAML y C#, fallback automático |
| Audio via MediaElement | Componente nativo del CommunityToolkit.Maui con soporte multiplataforma |
| SkiaSharp para gráficos | Procesamiento de imagen de alto rendimiento multiplataforma |

---

## 14. Plan de optimización

### Fase A — Corrección de issues de código (SENIOR)

| # | Issue | Archivo(s) | Severidad | Estado |
|---|-------|-----------|-----------|--------|
| 0 | Renombrar `PagesViewModel/` → `ViewModels/` | Carpeta + referencias | Baja | Pendiente |
| 1 | `CompassViewModel` no extiende `BaseViewModel` | `CompassViewModel.cs` | Alta | Pendiente |
| 2 | `ScreenLightViewModel.Cleanup()` oculta al padre sin `override` | `ScreenLightViewModel.cs` | Alta | Pendiente |
| 3 | Logs copy-paste en 5 ViewModels de instrumentos | `Instrument*ViewModel.cs` | Media | Pendiente |
| 4 | `ImageProcessingService.ProcessImageAsync` lanza `NotImplementedException` | `ImageProcessingService.cs` | Media | Pendiente |
| 5 | Código muerto/comentado en múltiples archivos | Varios | Baja | Pendiente |
| 6 | Carpeta `Behaviors/` vacía en `.csproj` | `.csproj` | Baja | Pendiente |
| 7 | ViewModels Singleton → Transient | `MauiProgram.cs` + ViewModels | Alta | Pendiente |
| 8 | `InstrumentStringComponent` resuelve DI manualmente | `InstrumentStringComponent.xaml.cs` | Media | Pendiente |
| 9 | `MetronomeService` usa `System.Timers.Timer` | `MetronomeService.cs` | Baja | Pendiente |
| 10 | `B_00_B0.wav` no referenciado | `InstrumentAudioService.cs` | Baja | Pendiente |

### Fase B — Documentación

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 11 | Actualizar `README.md` con estructura real y objetivo | `README.md` | Pendiente |

### Fase C — Separación Core + MAUI

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 12 | Crear proyecto `NavajaSuiza.Core` (Class Library) | `.csproj` + estructura | Pendiente |
| 13 | Mover modelos a Core (`InstrumentStringData`, `SupportedLanguages`) | Models | Pendiente |
| 14 | Mover interfaces de servicio a Core | Services/Interfaces | Pendiente |
| 15 | Mover `LanguageService` y `LocalizationResourceManager` a Core | Services + Extensions | Pendiente |
| 16 | Actualizar referencias en proyecto MAUI | `.csproj` | Pendiente |
| 17 | Verificar build completo | `dotnet build` | Pendiente |

### Fase D — Enriquecimiento de SKILL.md

| # | Tarea | Archivo | Estado |
|---|-------|---------|--------|
| 18 | Agregar sección MAUI completa al SKILL (transversal) | `SKILL.md` | Pendiente |

---

## 15. Hallazgos y deuda técnica

### 15.1 Bugs conocidos

1. **`CompassViewModel` no extiende `BaseViewModel`**: Extiende directamente `ObservableObject` en lugar de `BaseViewModel`, perdiendo `IsBusy`, `IsLoading`, `Title` y `Cleanup()`.

2. **Logs copy-paste en ViewModels de instrumentos**: Los logs de `InstrumentSteelViewModel`, `InstrumentBassViewModel`, `InstrumentUkuleleViewModel`, `InstrumentViolinViewModel` y `InstrumentCharangoViewModel` dicen `"Initializing Violin Strings in InstrumentNylonViewModel"` — claramente copiado del Nylon sin actualizar.

3. **`ImageProcessingService.ProcessImageAsync` lanza `NotImplementedException`**: La herramienta de Framing no puede procesar imágenes aún.

4. **`ScreenLightViewModel.Cleanup()` oculta a `BaseViewModel.Cleanup()`**: Falta `override` — el método nunca se invoca polimórficamente.

5. **README.md desactualizado**: La estructura de carpetas documentada no coincide con la real.

### 15.2 Mejoras detectadas

1. **Todos los servicios y páginas son Singleton**: ViewModels con estado (como `MetronomeViewModel` con `_currentBPM`) como Singleton pueden causar estado residual entre navegaciones.

2. **Carpeta `Behaviors/` vacía**: Creada en `.csproj` pero sin contenido.

3. **`InstrumentStringComponent` resuelve DI manualmente**: Usa `IPlatformApplication.Current.Services.GetService<>()` en `OnBindingContextChanged` en lugar de inyección por constructor (limitación de ContentView).

4. **MetronomeService usa `System.Timers.Timer`**: Requiere `MainThread.BeginInvokeOnMainThread` para tocar la UI.

5. **`TestingPage`/`TestingViewModel` vacíos**: Página de pruebas sin implementación.

6. **`B_00_B0.wav` no referenciado**: Archivo de audio del bajo que no está en la configuración de cuerdas.

7. **Comentarios código muerto**: Múltiples bloques comentados en `ThemeService`, `MetronomeService`, `FlashlightViewModel`, `InstrumentNylonViewModel` e `ImageProcessingService`.

---

## 16. Referencias

- **SKILL.md**: Guía transversal de patrones .NET Senior (APIs REST + MAUI).
- **AGENTS.md**: Reglas de operación para OpenCode en este proyecto.
- **README.md**: Documentación general del proyecto (pendiente de actualización).
