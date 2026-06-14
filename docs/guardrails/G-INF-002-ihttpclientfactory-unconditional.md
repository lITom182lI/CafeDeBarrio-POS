---
id: G-INF-002
type: anti-pattern
scope: PROYECTO
layer: infrastructure
trigger: IHttpClientFactory, AddHttpClient, condicional, Sunat, HealthCheck, DependencyInjection
linked-ledger: WARN-06
linked-adr: ADR-004
last-reviewed: 2026-06-11
review-when: Si se modifica la lógica de registro de servicios HTTP en DependencyInjection.cs
---

**Regla:** Llamar `builder.Services.AddHttpClient()` de forma incondicional en `Program.cs` antes de cualquier registro condicional de clientes HTTP tipados. (Nota: El orden fue corregido en TASK-04-BACKEND-httpclient-order).

**Why:** `SunatHealthCheck` requería `IHttpClientFactory`. En `DependencyInjection.cs`, `AddHttpClient<NubefactOseApiClient>()` solo se registraba si `Sunat:Enabled=true`. Con `Sunat:Enabled=false` (estado dev/local), `IHttpClientFactory` nunca se registraba en el contenedor DI. El API lanzaba `InvalidOperationException: Unable to resolve service for type 'IHttpClientFactory'` al arrancar.

**How to apply:** En `Program.cs`, antes de llamar a `builder.Services.AddInfrastructure()`:

```csharp
// CORRECTO: registro incondicional garantiza IHttpClientFactory en DI
builder.Services.AddHttpClient();
builder.Services.AddInfrastructure(builder.Configuration);

// INCORRECTO: depender de que el branch condicional registre AddHttpClient<T>
// Si Sunat:Enabled=false → IHttpClientFactory no registrado → crash al arrancar
```
