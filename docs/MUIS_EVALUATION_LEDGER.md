# MUIS EVALUATION LEDGER

**Fecha de evaluación base:** Junio 2026
**Proyecto:** CafeDeBarrio-POS
**Tipología Asignada:** Tipo 2 (SaaS Escalable)

Este documento es el registro inmutable de hallazgos arquitectónicos detectados durante la evaluación del sistema, según lo estipulado por el Protocolo Idempotente de Evaluación MUIS.

## 🟢 Hallazgos Resueltos (PASSED)

| ID | Capa | Descripción | Estado |
|---|---|---|---|
| F-01 | Infrastructure | `BaseRepository` violaba Unit of Work al llamar `SaveChangesAsync` directamente en `UpdateAsync` y `DeleteAsync`. | PASSED |
| F-02 | API / Boot | `EnsureCreated()` en lugar de `Migrate()` bypaseaba las migraciones de EF Core en Producción. | PASSED |
| F-03 | API / App | `AuthController` inyectaba repositorios directamente en lugar de delegar a MediatR (`LoginCommand`). | PASSED |
| F-04 | API / Boot | `UseForwardedHeaders` estaba posicionado erróneamente después de `UseHttpsRedirection`, causando loops de redirección en proxy. | PASSED |
| D-01 | Domain / DB | `Producto` ahora usa `RowVersion` como concurrency token para evitar stock negativo en ventas simultáneas. | PASSED |
| F-05 | Infrastructure | `ReportesRepository` agrupa en memoria (C#) después de un `ToListAsync()`. Migrado a SQL mediante GroupBy en IQueryable. | PASSED |
| F-06 | Root / Docs | Falta del archivo obligatorio `CLAUDE.md` con la Clasificación de Tipología (Regla 0). Se creó con Tipo 2 y datos de perfil. | PASSED |
| F-07 | Domain | `Producto` tenía duplicados los campos `FechaCreacion` y `FechaActualizacion`. Unificados en `IAuditable`. | PASSED |
| F-08 | Tests | Proyecto `CafeBarrio.Tests` (legacy) contenía solo `UnitTest1.cs`. Eliminado en favor de los proyectos Unit e Integration. | PASSED |
| F-09 | Docs | Domain Charter desactualizada sobre la integración SUNAT. Se creó versión `v1.1.0` reflejando el scope completo. | PASSED |
| POS-01 | API / App | Paginación bloqueaba el POS al solicitar 1000 items, violando el límite de 200 items. Se migró a Paginación Recursiva en Frontend. | PASSED |

---

## 🔴 Hallazgos Pendientes (PENDING)

### Arquitectura Backend & DDD

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
| A-01 | Application | **Falta de Idempotencia Offline:** `CreateTransaccionCommand` no acepta un Guid/IdempotencyKey desde el cliente (PWA). Si la red falla durante el sync, pueden generarse tickets duplicados. | Alto | PENDING |
| A-02 | Application | **Llamada a SUNAT síncrona en el hilo principal:** `CreateTransaccionHandler` espera a `_sunat.EmitirBoletaAsync()` antes de retornar. Violar la separación (Outbox Pattern) causa bloqueos en el POS si SUNAT se cae. | Crítico | PENDING |
| A-03 | Application / Domain | **Fuga de Lógica de Dominio:** El descuento de stock se hace manualmente en el handler (`producto.CantidadDisponible -= item.Cantidad`) en lugar de estar encapsulado en un método de la entidad `Producto`. | Medio | PENDING |

### Concurrencia y Datos

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
| D-02 | Application | **Tasa IGV Quemada (Hardcoded):** El `CreateTransaccionHandler` asume 10.5% como fallback si falla la config. | Bajo | PENDING |

### Frontend Web / Mobile

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
| UI-01 | Frontend | **Tipado Débil (any):** Uso explícito de `any` en componentes clave como `TerminalVentasView.tsx` y `ReportesYGraficos.tsx`, violando la regla MUIS de tipado estricto. | Medio | PENDING |

---

## 🟡 Hallazgos Diferidos (DEFERRED)

| ID | Capa | Descripción | Estado | Fecha Límite / Sprint |
|---|---|---|---|---|
| F-10 | Infrastructure | `JwtService` inyecta `IConfiguration` directo. Funciona en Tipo 1, pero se refactorizará a `IOptions<JwtOptions>`. | DEFERRED | Sprint V2 |

---

> *Este ledger debe ser actualizado conforme se solucionen los hallazgos. Ningún PR debe ser fusionado sin actualizar el estado a PASSED.*
