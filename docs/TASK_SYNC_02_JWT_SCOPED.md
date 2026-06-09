# TASK BUNDLE B — Paso 2: IJwtService Singleton → Scoped
**Generado por: Claude Code | Uso: pegar en Antigravity después del SESSION_BUNDLE_A.md**

> Este es el Bundle B. Ejecuta esta tarea exactamente como se describe. No agregues features fuera del scope.

---

## Identificación

| Campo          | Valor                        |
|----------------|------------------------------|
| ID de tarea    | TASK-02-jwt-scoped           |
| Fecha          | 2026-06-09                   |
| Módulo MUIS    | MUIS_BACKEND / MUIS_SECURITY_AUTH |
| Tier           | 0 (mecánico — una línea)     |
| Modelo destino | Gemini 3.5 Flash (Low)       |
| Plataforma     | Antigravity                  |

---

## Tu tarea

En `src/CafeBarrio.Infrastructure/DependencyInjection.cs` línea 38, cambiar el lifetime de `IJwtService` de `AddSingleton` a `AddScoped`. Luego ejecutar build y tests para confirmar que no hay regresiones.

---

## Inputs — Código de entrada

**Archivo a modificar:**
`src/CafeBarrio.Infrastructure/DependencyInjection.cs`

```csharp
using CafeBarrio.Application.Common.Interfaces;
using CafeBarrio.Infrastructure.Persistence;
using CafeBarrio.Infrastructure.Persistence.Repositories;
using CafeBarrio.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CafeBarrio.Infrastructure.Security;
using CafeBarrio.Infrastructure.External;

namespace CafeBarrio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<CafeBarrioDbContext>((sp, options) =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IOperadorRepository, OperadorRepository>();
        services.AddScoped<ISedeRepository, SedeRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();
        services.AddScoped<IAnulacionRepository, AnulacionRepository>();
        services.AddScoped<IMovimientoCajaRepository, MovimientoCajaRepository>();
        services.AddScoped<IConfiguracionNegocioRepository, ConfiguracionNegocioRepository>();
        services.AddScoped<ICategoriaCafeRepository, CategoriaCafeRepository>();
        services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
        services.AddScoped<IReportesRepository, ReportesRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<CafeBarrio.Application.Common.Interfaces.IJwtService, CafeBarrio.Infrastructure.Security.JwtService>();  // ← LÍNEA A CAMBIAR
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISunatService, SunatStubService>();

        return services;
    }
}
```

**Contexto — JwtService solo inyecta IConfiguration (Singleton):**
```csharp
// src/CafeBarrio.Infrastructure/Security/JwtService.cs
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;
    // ... genera tokens JWT, no tiene estado de instancia
}
```

---

## Restricciones

```
- Cambiar SOLO la línea 38: AddSingleton → AddScoped
- No modificar JwtService.cs
- No modificar IJwtService.cs
- No agregar nuevas dependencias a JwtService
- No cambiar ningún otro registro en DependencyInjection.cs
- Mantener el fully-qualified name en la línea modificada
```

---

## Output esperado

**Formato:** C# — archivo completo reemplazado  
**Archivos a entregar:** `src/CafeBarrio.Infrastructure/DependencyInjection.cs`

La línea 38 debe quedar exactamente así:
```csharp
services.AddScoped<CafeBarrio.Application.Common.Interfaces.IJwtService, CafeBarrio.Infrastructure.Security.JwtService>();
```

---

## Criterio de aceptación

```
✓ Línea 38 dice AddScoped (no AddSingleton)
✓ dotnet build src/CafeBarrio.sln --configuration Release → 0 errores, 0 warnings nuevos
✓ dotnet test tests/CafeBarrio.Tests.Unit → todos pasan
✓ Ningún otro archivo modificado
```

---

## Notas adicionales

El motivo del cambio es preventivo: `JwtService` actualmente solo inyecta `IConfiguration` (Singleton), por lo que no hay captive dependency hoy. Sin embargo, si en el futuro se inyecta `IHttpContextAccessor` o `ICurrentUserService` (ambos Scoped), tener `JwtService` como Singleton causaría un error de captive dependency en runtime. La política MUIS_SECURITY_AUTH exige que todos los servicios de seguridad que operan por-request sean Scoped.

---

> Cuando termines, entrega el archivo modificado y el VALIDATION REPORT.
> No expliques qué hiciste a menos que encuentres una ambigüedad.

---

## Output obligatorio — VALIDATION REPORT

Al terminar, genera y entrega este reporte en formato exacto:

```
TASK-02-jwt-scoped | [PASSED / FAILED] | 2026-06-09
Build: [N err] [N warn]
Criterios: [✓ todos] o listar solo los ✗
Archivos: [N creados / modificados / eliminados]
Desviaciones: Ninguna / [descripción]
Decisiones propias: Ninguna / [descripción]
Spot-check: No requerido / [archivo:línea — motivo]
```

El usuario copia este bloque y lo lleva a Claude para validación.
La tarea no está completa hasta que este reporte esté generado.
