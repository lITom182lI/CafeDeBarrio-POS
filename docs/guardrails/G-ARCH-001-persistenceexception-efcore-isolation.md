---
id: G-ARCH-001
type: validated-decision
scope: TIPO-2
layer: backend
trigger: DbUpdateException, Application, catch, EF Core, PersistenceException, Clean Architecture
linked-ledger: F-08
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se cambia la capa de persistencia (reemplazo de EF Core)"
---

**Regla:** Los handlers de la capa Application nunca capturan tipos de EF Core directamente. `DbUpdateException` se traduce a `PersistenceException` (dominio) en `UnitOfWork.SaveChangesAsync` (Infrastructure). Application solo captura `PersistenceException`.

**Why:** La capa Application no referencia `Microsoft.EntityFrameworkCore` — hacerlo viola Clean Architecture (dependencia hacia afuera de la cebolla). Durante TASK-27 (F-08) se intentó `catch (DbUpdateException)` en `DeleteProductoHandler` y generó CS0234. La alternativa temporal `when (ex.GetType().Name == "DbUpdateException")` funciona en runtime pero oculta la dependencia implícita y rompe el contrato de capas. La solución correcta es que Infrastructure traduzca la excepción, igual que ya hace con `DbUpdateConcurrencyException` → `ConcurrencyException` en `UnitOfWork.cs`.

**How to apply:** Al necesitar distinguir una violación de FK en un handler de Application:
1. `UnitOfWork.SaveChangesAsync` ya traduce `DbUpdateException` → `PersistenceException` (catch después del de `DbUpdateConcurrencyException` — orden importa, la subclase va primero).
2. El handler captura `PersistenceException` del namespace `CafeBarrio.Application.Common.Exceptions`.
3. Nunca usar `when (ex.GetType().Name == "...")` como sustituto — es un code smell de aislamiento roto.

```csharp
// ✅ Correcto — en DeleteProductoHandler.cs (Application)
catch (PersistenceException)
{
    producto.Activo = false;
    // soft delete fallback
}

// ❌ Incorrecto — referencia directa a EF Core en Application
catch (Microsoft.EntityFrameworkCore.DbUpdateException) { }

// ❌ Incorrecto — string comparison como workaround
catch (Exception ex) when (ex.GetType().Name == "DbUpdateException") { }
```
