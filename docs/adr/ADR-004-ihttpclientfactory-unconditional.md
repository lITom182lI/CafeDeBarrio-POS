# ADR-004: IHttpClientFactory registro incondicional en Program.cs

**Estado:** ACEPTADO
**Fecha:** 2026-06-11
**Guardrail relacionado:** G-INF-002
**Ledger relacionado:** WARN-06

---

## Contexto

`SunatHealthCheck` depende de `IHttpClientFactory` para verificar conectividad con el OSE de SUNAT. En `DependencyInjection.cs`, el cliente tipado `NubefactOseApiClient` se registra condicionalmente cuando `Sunat:Enabled=true`. En entornos con `Sunat:Enabled=false` (desarrollo local), el branch condicional nunca se ejecuta y `IHttpClientFactory` nunca se registra en el contenedor DI. El API lanzaba `InvalidOperationException: Unable to resolve service for type 'IHttpClientFactory'` al arrancar.

## Decisión

Agregar `builder.Services.AddHttpClient()` de forma incondicional en `Program.cs`, antes de llamar a `AddInfrastructure()`. Esto garantiza que `IHttpClientFactory` siempre esté disponible en el contenedor DI independientemente del valor de `Sunat:Enabled`.

## Alternativas consideradas

| Alternativa | Razón de descarte |
|---|---|
| Registrar `IHttpClientFactory` solo en el branch `Sunat:Enabled=true` | `SunatHealthCheck` siempre se registra en el pipeline de health checks; necesita el factory para el modo stub. No registrarlo causa error en arranque. |
| Inyectar `IHttpClientFactory` como nullable | Complica el constructor del health check y enmascara un problema de configuración real. |
| Mover `SunatHealthCheck` al branch condicional | El health check debe existir siempre para reportar el estado del stub-mode; ocultarlo cuando SUNAT está deshabilitado elimina información operativa útil. |

## Consecuencias

### Positivas
- El contenedor DI siempre tiene `IHttpClientFactory` disponible
- `SunatHealthCheck` funciona en cualquier configuración de `Sunat:Enabled`

### Negativas / Trade-offs aceptados
- `AddHttpClient()` incondicional registra la infraestructura de HttpClient aunque no haya clientes tipados activos — overhead mínimo y aceptable

## Trigger de revisión

Si se refactoriza la integración SUNAT o se cambia el mecanismo de health checks HTTP.

## Inmutabilidad

Inmutable a partir de su aceptación. Si se refactoriza DI, crear ADR que reemplace a este.
