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
| F-06 | Root / Docs | Falta del archivo obligatorio `CLAUDE.md` con la Clasificación de Tipología (Regla 0). Se creó con Tipo 2 y datos de perfil. | PASSED |
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
| D-01 | Domain / DB | **Condición de Carrera en Inventario:** La entidad `Producto` no posee un token de concurrencia (`RowVersion`). Dos ventas simultáneas del último ítem dejarán el stock en negativo. | Alto | PENDING |
| D-02 | Application | **Tasa IGV Quemada (Hardcoded):** El `CreateTransaccionHandler` asume 10.5% como fallback si falla la config. | Bajo | PENDING |

### Frontend Web / Mobile

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
| UI-01 | Frontend | **Tipado Débil (any):** Uso explícito de `any` en componentes clave como `TerminalVentasView.tsx` y `ReportesYGraficos.tsx`, violando la regla MUIS de tipado estricto. | Medio | PENDING |

---

## 🟡 Hallazgos Diferidos (DEFERRED)

| ID | Capa | Descripción | Estado | Fecha Límite / Sprint |
|---|---|---|---|---|
| F-05 | Infrastructure | `ReportesRepository` agrupa en memoria (C#) después de un `ToListAsync()`. Aceptable para volumen bajo, pero debe migrarse a SQL. | DEFERRED | Sprint V2 |

---

> *Este ledger debe ser actualizado conforme se solucionen los hallazgos. Ningún PR debe ser fusionado sin actualizar el estado a PASSED.*
