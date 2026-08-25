# SKILL: .NET (C#) — Patrón Senior (transversal)

Guía de referencia para construir APIs REST en **.NET (ASP.NET Core) + Dapper + SQL Server** siguiendo una arquitectura y convenciones senior validadas en producción (`WebApiCore`, Clean Architecture). Es **transversal**: los ejemplos son genéricos (auth, CRUD, manejo de errores, configuración) y aplican a cualquier dominio. Cubre también buenas prácticas para clientes **MAUI** (MVVM) porque comparten las mismas reglas de C#, seguridad y testing.

Este archivo es un **skill**: se lee para replicar el patrón en cualquier proyecto .NET nuevo. No es una receta dogmática; es la lista de decisiones que convierten un CRUD simple en un backend mantenible, seguro y desplegable.

---

## 1. ¿Por qué este patrón es SENIOR?

Porque resuelve los problemas que matan a las APIs .NET cuando crecen, con decisiones **justificadas**, no por moda:

| Decisión | Problema que resuelve |
|----------|----------------------|
| **Clean Architecture por capas** (`Domain` → `Application` → `Infrastructure` → `API`) | Dependencias en una sola dirección; la API no conoce repositorios, los servicios no conocen Dapper. Cambiar de ORM o de BD no toca la capa de aplicación |
| **Envelope uniforme `ApiResponse<T>`** | Toda respuesta (éxito y error) tiene el mismo contrato `{isSuccess, statusCode, message, data, errors}`. El frontend tiene un solo patrón de consumo |
| **`GlobalExceptionHandler` → 500 genérico + `ProblemDetails`** | El detalle real de la excepción va al log, nunca al cliente. Sin fuga de stack traces ni internos |
| **Fail-fast de configuración** | Config inválida (connection string faltante, CORS vacío, JWT sin sección) → excepción al arrancar, no fallas en runtime difíciles de diagnosticar |
| **Connection string por entorno** (`Development` → `SqlServer`, resto → `SqlServerWeb`) | El mismo código corre en local y en producción sin tocar el repositorio; la config correcta la decide el entorno |
| **JWT identifica + `ApiKey` global** | Separa "quién puede llamar a la API" (ApiKey del origen, validada contra BD) de "quién es el usuario" (JWT) |
| **Rate limiting por cliente (`X-Forwarded-For` → IP)** | Protección de fuerza bruta que no bloquea a todos los usuarios por igual |
| **Contraseñas con PBKDF2 (KDF)** | Hash seguro con salt e iteraciones configurables; nunca almacenar texto plano ni MD5/SHA simples |
| **Stored Procedures + Dapper** | La lógica de datos vive en la BD (reutilizable, auditable); Dapper es simple y sin magic strings del ORM |
| **Tests de integración con BD real** | Validan el flujo completo (DTO → SP → respuesta) contra la base real, no contra mocks que mienten |
| **Sin secretos en el código** | Connection strings, claves JWT y ApiKeys van en configuración/secrets del entorno, nunca hardcodeadas ni en el repo |

---

## 2. Stack recomendado

| Capa | Tecnología | Nota |
|------|-----------|------|
| API | ASP.NET Core (net8/net9/net10 según contexto) | Web API con Controllers, no minimal API para CRUD corporativo |
| ORM | **Dapper** + `System.Data.SqlClient` | Ligero, explícito, sin tracking |
| BD | SQL Server + Stored Procedures | Lógica de datos en la BD |
| Auth | JWT (Microsoft.AspNetCore.Authentication.JwtBearer) + ApiKey propio | Filter/attr |
| Rate limiting | ASP.NET Core RateLimiter | Fixed window + partition por IP |
| Logs | ILogger + GlobalExceptionHandler | Sin librería de terceros necesaria |
| Tests | xUnit + WebApplicationFactory | Integración contra BD real |
| Serialización | System.Text.Json | CamelCase, sin ciclos |
| Documentación | Swagger/Swashbuckle | Versionado compatible (ver §9) |

---

## 3. Estructura de carpetas (Clean Architecture)

```
WebApiCore.sln
├── WebApiCore.Domain/          # Modelos, DTOs, entidades (sin dependencias)
│   └── Models/                 # e.g. ApiResponse<T>, User, LoginRequest
├── WebApiCore.Application/     # Servicios y lógica de negocio
│   └── Services/               # e.g. UserService, AuthService
├── WebApiCore.Infrastructure/  # Acceso a datos (Dapper, context, SPs)
│   ├── Repositories/
│   ├── Context/                # IDapperContext
│   └── Mappings/
└── WebApiCore/                 # API (Program.cs, Controllers, Filters)
    ├── Controllers/
    ├── Filters/                # ApiKeyOperationFilter, AuthorizeOperationFilter, GlobalExceptionHandler
    ├── Middleware/
    ├── Models/
    └── appsettings.json        # + appsettings.{Environment}.json
```

Reglas de dependencia (una sola dirección):
- `Domain` no conoce a nadie.
- `Application` conoce a `Domain`.
- `Infrastructure` conoce a `Application` (y `Domain`).
- `WebApiCore` (API) conoce a todos, pero **nadie la conoce a ella**.

**Anti-patrón clásico**: un solo proyecto `WebApiCore` con `Models/Repositories/Services/Controllers` todos juntos. Funciona el primer año; luego las dependencias se mezclan y migrar de ORM o de BD rompe todo.

---

## 4. Capa de datos (Dapper + SPs)

### Contexto (IDapperContext)

```csharp
public interface IDapperContext
{
    string ConnectionString { get; }
}
```

Implementación que toma la connection string del entorno activo:

```csharp
public class DapperContext : IDapperContext
{
    public string ConnectionString { get; }

    public DapperContext(IConfiguration config, IHostEnvironment env)
    {
        // Fail-fast: si falta, la API no arranca.
        var conn = env.IsDevelopment()
            ? config.GetConnectionString("SqlServer")
            : config.GetConnectionString("SqlServerWeb");

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("No se encontró ConnectionStrings:SqlServer/SqlServerWeb");
        ConnectionString = conn;
    }
}
```

### Stored Procedures con contrato de respuesta (SqlResponse)

Cada SP devuelve un resultado con `IsSuccess`, `StatusCode` y `Message` para que la capa de aplicación sepa si la operación fue exitosa sin adivinar:

```sql
-- Patrón de SP de escritura
CREATE PROCEDURE Auth_Login
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(500),
    @IsSuccess BIT OUTPUT,
    @StatusCode INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Auth_Users WHERE Email = @Email)
    BEGIN
        SET @IsSuccess = 0; SET @StatusCode = 400;
        SET @Message = N'Usuario o contraseña incorrecta'; RETURN;
    END
    -- validar hash...
END
```

### Uso desde repositorio

```csharp
public async Task<LoginResult?> GetUserAsync(string email, string passwordHash)
{
    using var connection = new SqlConnection(_context.ConnectionString);
    var p = new DynamicParameters();
    p.Add("@Email", email);
    p.Add("@PasswordHash", passwordHash);
    p.Add("@IsSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
    p.Add("@StatusCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
    p.Add("@Message", dbType: DbType.String, size: 500, direction: ParameterDirection.Output);

    await connection.ExecuteAsync("Auth_Login", p, commandType: CommandType.StoredProcedure);

    var isSuccess = p.Get<bool>("@IsSuccess");
    if (!isSuccess)
        return new LoginResult(false, p.Get<int>("@StatusCode"), p.Get<string>("@Message"));

    // segunda consulta para datos del usuario...
    return new LoginResult(true, 200, "OK", user);
}
```

**Regla**: los repositorios reciben datos ya procesados (por ejemplo, el `passwordHash` calculado en Application); la capa de datos no aplica lógica de negocio.

---

## 5. Contrato uniforme de respuesta (Envelope)

Toda respuesta HTTP pasa por el mismo envelope. Esto estabiliza el contrato con el frontend:

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
```

Controladores lo usan siempre, sin excepciones:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var user = await _userService.GetByIdAsync(id);
    if (user is null)
        return NotFound(new ApiResponse<object> { IsSuccess = false, StatusCode = 404, Message = "No encontrado" });

    return Ok(new ApiResponse<UserDto> { IsSuccess = true, StatusCode = 200, Message = "OK", Data = user });
}
```

### Tabla de códigos coherente

| Caso | HTTP | Envelope |
|------|------|----------|
| Éxito | 200/201 | `IsSuccess=true` |
| Entrada inválida (modelo/validación) | 400 | `IsSuccess=false`, `Errors` |
| No autenticado | 401 | `IsSuccess=false` |
| Sin permiso | 403 | `IsSuccess=false` |
| No existe | 404 | `IsSuccess=false` |
| Límite de peticiones excedido | 429 | `IsSuccess=false` |
| Error interno | 500 | `IsSuccess=false`, mensaje genérico |

**Anti-patrón**: devolver en unos endpoints `{data: ...}` directo y en otros un `string` de error o `ProblemDetails` crudo. El frontend termina con `if (status === 400) ... else if (typeof res === 'string') ...`.

---

## 6. Manejo de errores global

### `GlobalExceptionHandler`

Un solo manejador central captura excepciones no controladas, las **loguea** y responde 500 genérico:

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Error no controlado");
        var response = new ApiResponse<object>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = "Ha ocurrido un error interno."
        };
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(response, ct);
        return true;
    }
}
```

Registro:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// ...
app.UseExceptionHandler();
```

### Error de modelo (400) uniforme

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opts =>
    {
        opts.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return new BadRequestObjectResult(new ApiResponse<object>
            {
                IsSuccess = false, StatusCode = 400,
                Message = "Datos de entrada inválidos.", Errors = errors
            });
        };
    });
```

**Regla**: el mensaje de 500 es genérico para el cliente; el detalle real va al log. Nunca `ex.Message` en una respuesta 500.

---

## 7. Autenticación y autorización

### Contraseñas (PBKDF2)

```csharp
public static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(16);
    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
        password, salt, 10000, HashAlgorithmName.SHA256, 32);
    return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
}
```

**Nunca**: texto plano, MD5, SHA1, o el mismo hash para todos los usuarios (sin salt).

### JWT

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:Key"]!)),
            ValidIssuer = config["JWT:Issuer"],
            ValidAudience = config["JWT:Audience"]
        };
        // 401 uniforme si falla el token
        opts.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsJsonAsync(new ApiResponse<object>
                { IsSuccess = false, StatusCode = 401, Message = "No autorizado." });
            }
        };
    });
```

### ApiKey global (separada del JWT)

El origen (Swagger, Postman, frontend) envía la ApiKey configurada en BD (`Mae_Config`). Se valida en un filtro de acción:

```csharp
public class ApiKeyFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
        {
            ctx.Result = new UnauthorizedObjectResult(new ApiResponse<object>
                { IsSuccess = false, StatusCode = 401, Message = "ApiKey requerida." });
            return;
        }
        // comparar contra el valor almacenado en BD (Mae_Config) de forma segura
        var stored = await _configService.GetAsync("ApiKey");
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(apiKey!),
                Encoding.UTF8.GetBytes(stored)))
        {
            ctx.Result = new UnauthorizedObjectResult(/* 401 uniforme */);
            return;
        }
        await next();
    }
}
```

**Regla**: la ApiKey viaja en header, **nunca en la query string** (queda en logs del servidor y del proxy).

---

## 8. Rate limiting y CORS

### Rate limiting por cliente

```csharp
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = 429;
    opts.AddFixedWindowLimiter("fixed", lim =>
    {
        lim.PermitLimit = 10;
        lim.Window = TimeSpan.FromMinutes(1);
        lim.QueueLimit = 0;
        lim.PartitionKey = PartitionKey.Get<HttpContext>(ctx =>
        {
            // detrás de proxy/load balancer
            var fwd = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(fwd)
                ? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                : fwd;
        });
    });
});
// ...
app.UseRateLimiter();
// en controladores sensibles:
[EnableRateLimiting("fixed")]
```

**Anti-patrón**: rate limit global sin partición por IP → un solo usuario abusivo bloquea a todos, o la regla no aplica tras un proxy porque todos llegan con la IP del balanceador.

### CORS (allow-list explícita)

```csharp
var cors = config.GetSection("Cors").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("Default", p =>
    p.WithOrigins(cors).AllowAnyHeader().AllowAnyMethod()));
```

**Regla**: `WithOrigins` con lista explícita. **Nunca** `SetIsOriginAllowed(_ => true)` ni `AllowAnyOrigin()` en producción, salvo API pública documentada.

---

## 9. Configuración y fail-fast

`appsettings.json` (producción) y `appsettings.Development.json` (local) por separado:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=LOCAL;Database=...;Integrated Security=True",
    "SqlServerWeb": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True"
  },
  "JWT": { "Key": "...", "Issuer": "...", "Audience": "...", "ExpireMinutes": 60 },
  "Cors": [ "https://origen-frontend" ],
  "RateLimit": { "PermitLimit": 10, "WindowMinutes": 1 }
}
```

Fail-fast: validar la config requerida **al arrancar**, no al primer request:

```csharp
if (string.IsNullOrWhiteSpace(config["JWT:Key"])) throw new InvalidOperationException("JWT:Key no configurada");
```

**Regla de oro**: un servidor que arranca con config inválida es un bug silencioso; uno que lanza excepción es un bug evidente.

**Nunca** commitear `appsettings.json` con secretos reales. Usar `.env`/secrets del proveedor o variables de entorno y mantener en el repo una plantilla `appsettings.Example.json`.

---

## 10. Swagger / OpenAPI (compatibilidad de versiones)

La versión de Swashbuckle define la API de `Microsoft.OpenApi`:

| Swashbuckle | Microsoft.OpenApi | Nota |
|-------------|-------------------|------|
| 6.x | 1.x | `OpenApiSchema`, `OpenApiReference` (legado, aún válido) |
| 9.x | 1.x | Igual que 6.x pero más actualizada |
| 10.x | **2.x** | **Breaking**: `OpenApiSchema` no existe, `Type` es `JsonSchemaType?`, `OpenApiReference` eliminado |

### Ejemplo de filter compatible (OpenApi 2.x, Swashbuckle 10+)

```csharp
// en OpenApi 2.x el namespace Microsoft.OpenApi.Models fue reemplazado por Microsoft.OpenApi
var schemeRef = new OpenApiSecuritySchemeReference("Bearer");
operation.Security = [new OpenApiSecurityRequirement { { schemeRef, new List<string>() } }];
// para OpenApi 1.x (6.x/9.x) se usaba:
//   new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
```

Endpoint de Swagger en `Program.cs` — usar **ruta absoluta** para evitar problemas tras proxies/host virtuales:

```csharp
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiCore v1"));
```

**Gotchas reales detectados**:
- Si `index.html`/`index.js` de Swagger se sirven con `Cache-Control` largo, el navegador cachea la UI vieja y muestra errores como *"does not specify a valid version field"* aunque el JSON sea válido. Probar con **Ctrl+Shift+R** (hard reload) o incógnito antes de concluir que el spec está roto.
- Validar el spec en `/swagger/v1/swagger.json` directamente, no la página HTML.
- Si Swashbuckle 9.x da problemas de render (UI rota en producción), **bajar a 6.6.2** es una solución validada que conserva todo el spec.

---

## 11. MAUI (clientes .NET multiplataforma)

Las mismas reglas de C# y seguridad aplican al frontend MAUI. Anti-patrones que rompen apps reales:

| Anti-patrón | Problema | Solución |
|-------------|----------|----------|
| `NotImplementedException` en métodos de servicio | La app "funciona" hasta que alguien toca ese botón → crash | Implementar o eliminar; si es placeholder, marcarlo explícitamente |
| `Application.Current.Windows[0].Page` para navegar | Navegación acoplada a la ventana, rompe con más de una ventana | `Shell.Current` / inyección de navegación (MVVM) |
| `catch (Exception) { }` vacío | Traga errores; el usuario ve que "no pasa nada" | Log + estado de error visible en UI |
| Operaciones `async void` fire-and-forget | Excepciones sin controlar crashean la app | `async Task`, `Command`, try/catch central |
| Crear `HttpClient` por cada llamada | Agotamiento de sockets | `HttpClient` singleton/inyectado |
| `HttpClientHandler` manual con SSL bypass | MITM → fuga de credenciales | Configuración de trust del SO, nunca `ServerCertificateCustomValidationCallback = (_) => true` |
| `System.Random` para contraseñas/IDs | Predictible, inseguro | `RandomNumberGenerator` |
| Columnas `Data01`/`Data02` en BD | Sin semántica, imposible de mantener | Nombres de dominio reales |
| `System.Timers.Timer` tocando la UI desde otro hilo | Race conditions / crashes de UI | `Dispatcher`/`MainThread.InvokeOnMainThreadAsync` |
| ViewModel Singleton con estado global compartido | Estado corrupto entre páginas | ViewModel por página, servicios como singletons |

### Reglas MAUI senior
- **MVVM**: ViewModel por página, propiedades `ObservableProperty`, `[RelayCommand]`.
- **Inyección de dependencias** (DI nativa de MAUI): servicios en `MauiProgram`, páginas/VM resueltas por DI.
- **Nunca** lógica de negocio en `code-behind`; solo eventos de UI delegando a comandos.
- **HttpClient singleton + auth** con handlers que agregan JWT/ApiKey.
- Tratar la migración/refactor como un **proyecto de auditoría**: leer el análisis previo (por ejemplo `ANALISIS_V1.md`) y corregir los hallazgos uno a uno con aprobación del usuario (regla de issues).

---

## 12. Tests

- **xUnit + `WebApplicationFactory<T>`** para tests de integración de la API.
- Probar **contra BD real** (o instancia de prueba) para validar DTO → SP → respuesta completa.
- Verificar el **envelope**: `IsSuccess`, `StatusCode`, `Message` correctos para éxito, 400, 401, 404, 429 y 500.
- No mockear repositorios para probar la API: el valor está en el flujo real.
- Comando: `dotnet test` (no ejecutar sin autorización del usuario según las reglas del repo).

---

## 13. Checklist final

- [ ] Estructura Clean Architecture con dependencias en una sola dirección.
- [ ] Envelope `ApiResponse<T>` en **todas** las respuestas (éxito y error).
- [ ] `GlobalExceptionHandler` central: log + 500 genérico, sin fuga de internos.
- [ ] 400 uniforme vía `InvalidModelStateResponseFactory`.
- [ ] Contraseñas con PBKDF2 + salt + iteraciones configurables.
- [ ] JWT con `ClockSkew=0`, issuer/audience validados, key desde config.
- [ ] ApiKey en header (nunca en query string) validada contra BD.
- [ ] Rate limiting particionado por IP con `X-Forwarded-For`.
- [ ] CORS con allow-list explícita; sin `AllowAnyOrigin` en producción.
- [ ] Fail-fast de configuración al arrancar.
- [ ] No secretos hardcodeados ni en el repo.
- [ ] Swagger con ruta absoluta y versión de Swashbuckle compatible con OpenApi (6.x/9.x vs 10.x).
- [ ] `dotnet build` sin errores ni warnings.
- [ ] Tests de integración cubriendo los códigos del envelope.
- [ ] Verificación real en runtime (navegador/Swagger) tras el deploy; no basta que compile.
- [ ] Documentar decisiones relevantes en `DEVELOPMENT.md` del proyecto.

---

## 14. Anti-patrones generales (resumen rápido)

| Anti-patrón | Solución |
|-------------|----------|
| Todo en un solo proyecto API | Clean Architecture por capas |
| Respuestas HTTP inconsistentes | Envelope único `ApiResponse<T>` |
| `ex.Message` al cliente en 500 | Log + mensaje genérico |
| Config inválida detectada en runtime | Fail-fast al arrancar |
| Secretos en el código/repo | Config/secrets del entorno |
| `AllowAnyOrigin` / `SetIsOriginAllowed(_=>true)` | Allow-list explícita |
| Hash sin salt o MD5/SHA para contraseñas | PBKDF2 con salt (KDF) |
| Fire-and-forget / `async void` | `async Task` + manejo central |
| `catch {}` vacío | Log + estado visible en UI |
| `HttpClient` nuevo por llamada | Singleton inyectado |
| SSL bypass en el cliente | Trust del SO; nunca `_ => true` |
| Migrar de versión sin revisar breaking changes | Verificar matriz de versiones (p.ej. OpenApi 2.x) antes del upgrade |

---

## 15. Referencias del patrón validado

- **Repo de referencia**: `WebApiCore` (Clean Architecture + Dapper + JWT + ApiKey + rate limit + envelope) — deployado y operativo.
- **Migration .NET 10**: `PasswordManager_.NET10` — port del mismo patrón con OpenApi 2.x (Swashbuckle 10.x).
- **Config de despliegue**: connection string de producción por entorno, fail-fast, CORS por allow-list.
- **Documentación**: ver `DEVELOPMENT.md` del proyecto para decisiones de diseño y alternativas descartadas.
