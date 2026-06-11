---
id: G-INF-003
type: validated-decision
scope: TIPO-2
layer: infrastructure
trigger: HasData, seed, catálogo, productos, categorías, datos iniciales, migración
linked-ledger: PROD-05
linked-adr: ADR-005
last-reviewed: 2026-06-11
review-when: Si se agregan nuevas entidades con datos de catálogo de negocio
---

**Regla:** No usar `HasData()` de EF Core para datos de catálogo o negocio — usar siempre un `DataSeeder` idempotente invocado al startup.

**Why:** `HasData()` bake datos en migraciones. Cualquier cambio de precio, nombre o nuevo producto requiere una migración nueva. `RemoveConfigSeed` en el historial de migraciones del proyecto documenta que esto fue deliberadamente removido. Para un sistema POS con catálogo vivo, es insostenible.

**How to apply:** Para cualquier dato de catálogo o configuración inicial:

```csharp
// CORRECTO: seeder idempotente
public class MiSeeder : IMiSeeder
{
    public async Task SeedAsync(CancellationToken ct)
    {
        if (await _context.MiEntidad.AnyAsync(ct)) return; // idempotente
        _context.MiEntidad.AddRange(/* datos */);
        await _context.SaveChangesAsync(ct);
    }
}
// Registrar como Scoped en DI y llamar desde Program.cs después de UseAuthorization()

// INCORRECTO: precio hardcodeado en migración
builder.HasData(new Producto { Id = 1, Precio = 7.00m });
```
