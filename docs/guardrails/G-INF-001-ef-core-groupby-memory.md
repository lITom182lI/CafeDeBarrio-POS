---
id: G-INF-001
type: anti-pattern
scope: PROYECTO
layer: infrastructure
trigger: EF Core, GroupBy, fecha, Date, IQueryable, ReportesRepository, ventas-por-dia
linked-ledger: F-05
linked-adr: ADR-003
last-reviewed: 2026-06-11
review-when: Upgrade a EF Core 10+ — verificar si la limitación de traducción fue resuelta
---

**Regla:** No usar `IQueryable.GroupBy(t => t.Campo.Date)` combinado con filtro de propiedad de navegación nula (`t.Navegacion == null`) — EF Core 9 no puede traducirlo a SQL.

**Why:** `GetVentasPorDiaAsync` en `ReportesRepository` usaba `GroupBy(t => t.Fecha.Date)` con `.Where(t => t.Anulacion == null)` en IQueryable. EF Core 9 lanzaba `InvalidOperationException` en runtime porque no puede traducir la combinación de expresión de fecha + filtro de navegación nula a SQL válido. Endpoint `/api/reportes/ventas-por-dia` retornaba HTTP 500.

**How to apply:** Antes de escribir cualquier `GroupBy` en `IQueryable` que use `.Date`, `.Hour`, `.Month` u otras expresiones de fecha/hora, verificar este guardrail. Patrón validado:

```csharp
// CORRECTO: filtrar + proyectar en SQL, agrupar en memoria
var rows = await _context.Entidad
    .Where(filtros)                              // filtros en SQL
    .Select(t => new { t.Fecha, t.Total })       // proyección mínima en SQL
    .ToListAsync(ct);                            // materializar
return rows
    .GroupBy(t => t.Fecha.Date)                  // agrupar en memoria C#
    .Select(g => new Dto(g.Key, g.Sum(...)))
    .OrderBy(x => x.Fecha)
    .ToList();

// INCORRECTO: GroupBy en IQueryable con expresión de fecha
_context.Entidad
    .Where(t => t.Navegacion == null)
    .GroupBy(t => t.Fecha.Date)   // falla en EF Core 9
    .Select(...)
    .ToListAsync();
```
