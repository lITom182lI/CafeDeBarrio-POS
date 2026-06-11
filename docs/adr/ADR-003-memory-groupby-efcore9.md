# ADR-003: Memory GroupBy sobre SQL GroupBy para consultas de fecha en EF Core 9

**Estado:** ACEPTADO
**Fecha:** 2026-06-11
**Guardrail relacionado:** G-INF-001
**Ledger relacionado:** F-05

---

## Contexto

`GetVentasPorDiaAsync` en `ReportesRepository` necesitaba agrupar transacciones por fecha para el endpoint `/api/reportes/ventas-por-dia`. La implementación inicial (registrada en F-05 del Ledger como "migrado a SQL") intentó usar `IQueryable.GroupBy()` para ejecutar la agrupación en el motor de base de datos. En entorno local el endpoint retornaba HTTP 500 con `InvalidOperationException`.

## Decisión

Materializar las filas necesarias con proyección mínima (`Select(t => new { t.Fecha, t.Total })`) y filtrado completo en SQL mediante `ToListAsync()`, luego ejecutar el `GroupBy` sobre la colección en memoria C#.

## Alternativas consideradas

| Alternativa | Razón de descarte |
|---|---|
| SQL GroupBy en IQueryable | EF Core 9 no puede traducir `GroupBy(t => t.Fecha.Date)` combinado con filtro de propiedad de navegación nula (`t.Anulacion == null`) a SQL válido. Lanza `InvalidOperationException` en runtime. |
| Raw SQL con interpolación | Rompe la abstracción del repositorio, dificulta el testing y acopla la infraestructura al dialecto SQL Server. |
| Vista o stored procedure | Introduce una capa de gestión adicional en la BD incompatible con el modelo de migraciones EF Core del proyecto. |

## Consecuencias

### Positivas
- Funciona correctamente con EF Core 9
- El `Where` y el `Select` se ejecutan en SQL — solo los campos y filas necesarios viajan por la red
- El código es testeable en memoria sin BD real

### Negativas / Trade-offs aceptados
- El `GroupBy` final ocurre en memoria C# — aceptable con el volumen de transacciones por sede/día esperado en este sistema

## Trigger de revisión

Upgrade a EF Core 10+ — verificar si la limitación de traducción de `GroupBy` sobre expresiones de fecha fue resuelta.

## Inmutabilidad

Inmutable a partir de su aceptación. Si EF Core 10+ resuelve la limitación, crear ADR-007 que reemplace a este.
