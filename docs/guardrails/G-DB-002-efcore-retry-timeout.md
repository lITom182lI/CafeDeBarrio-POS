---
id: G-DB-002
type: validated-decision
scope: TIPO-2
layer: infrastructure
trigger: UseSqlServer, EnableRetryOnFailure, CommandTimeout, resiliencia, BD, pool
linked-ledger: F-04
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se migra a otro proveedor de BD o se cambia la estrategia de resiliencia"
---

**Regla:** `UseSqlServer` siempre se configura con `EnableRetryOnFailure` (5 reintentos, 10s delay) y `CommandTimeout(30)`. Sin esta configuración, cualquier microcorte de red falla la petición sin reintento.

**Why:** En F-04 `DependencyInjection.cs` llamaba `UseSqlServer` sin configuración de resiliencia. Los errores transitorios 1205 (deadlock), 40613 (Azure throttle) y timeouts fallaban inmediatamente en lugar de reintentarse.

**How to apply:**
```csharp
// ✅ Correcto
options.UseSqlServer(connectionString, sql =>
{
    sql.EnableRetryOnFailure(
        maxRetryCount:     5,
        maxRetryDelay:     TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null);
    sql.CommandTimeout(30);
});

// ❌ Incorrecto
options.UseSqlServer(connectionString);
```
Nota: al activar `EnableRetryOnFailure`, `UnitOfWork.BeginTransactionAsync` ya maneja correctamente la transacción ambiente en tests — no requiere cambios adicionales.
