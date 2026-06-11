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
| D-02 | Application | Se eliminó el IGV hardcodeado en `CreateTransaccionHandler`. Ahora retorna falla si no encuentra configuración de la sede. | PASSED |
| A-03 | Application / Domain | Se encapsuló la lógica de descuento de inventario dentro del método `DescontarStock` en la entidad `Producto`. | PASSED |
| A-01 | Application | Implementado IdempotencyKey en `Transaccion` para prevenir la duplicación de tickets en PWA por reintentos de red. | PASSED |
| A-02 | Application / Infra | Llamada a SUNAT separada del hilo principal mediante el Patrón Outbox y un `BackgroundService`. La venta se procesa y responde inmediatamente. | PASSED |
| F-05 | Infrastructure | `ReportesRepository` agrupa en memoria (C#) después de un `ToListAsync()`. Migrado a SQL mediante GroupBy en IQueryable. | PASSED |
| F-06 | Root / Docs | Falta del archivo obligatorio `CLAUDE.md` con la Clasificación de Tipología (Regla 0). Se creó con Tipo 2 y datos de perfil. | PASSED |
| F-07 | Domain | `Producto` tenía duplicados los campos `FechaCreacion` y `FechaActualizacion`. Unificados en `IAuditable`. | PASSED |
| F-08 | Tests | Proyecto `CafeBarrio.Tests` (legacy) contenía solo `UnitTest1.cs`. Eliminado en favor de los proyectos Unit e Integration. | PASSED |
| F-09 | Docs | Domain Charter desactualizada sobre la integración SUNAT. Se creó versión `v1.1.0` reflejando el scope completo. | PASSED |
| POS-01 | API / App | Paginación bloqueaba el POS al solicitar 1000 items, violando el límite de 200 items. Se migró a Paginación Recursiva en Frontend. | PASSED |
| PROD-01 | API / Boot | Sin validación de config crítica al startup: JWT Key, ConnectionString y CORS podían arrancar con valores placeholder. Guard `RequireConfig` añadido en `Program.cs` (lanza en non-Development). | PASSED |
| PROD-02 | Infrastructure | `SunatEmisionService` reintentaba indefinidamente en caso de fallo de infraestructura. Implementado `MaxRetries = 3` + estado `DeadLetter` + columna `SunatIntentos`. Migración `S10_SunatIntentos`. | PASSED |
| PROD-03 | DevOps | Sin imagen Docker. Creados `Dockerfile` multi-stage (sdk:9.0 → aspnet:9.0, non-root `appuser`, puerto 8080) y `.dockerignore`. | PASSED |
| PROD-04 | DevOps / CI | Sin pipeline de entrega continua. Job `docker-publish` añadido a `ci.yml`: construye y publica imagen a `ghcr.io` en cada push a `main` (solo si `build-and-test` pasa). | PASSED |
| WARN-01 | API / Boot | CORS usaba `AllowAnyHeader` + `AllowAnyMethod`. Restringido a `WithHeaders("Content-Type","Authorization","X-Operator-Id")` y `WithMethods("GET","POST","PUT","DELETE")`. | PASSED |
| WARN-02 | API / Security | Rate limiting ausente en endpoints de escritura. Política `api-write-policy` (200 req/min/IP) aplicada a `POST /api/transacciones` y `POST /api/productos`. | PASSED |
| WARN-03 | Frontend | `const SEDE = 1` hardcodeado en dashboard. Migrado a `import.meta.env.VITE_SEDE_ID`. Creados `dashboard/.env` y `dashboard/.env.production`. | PASSED |
| WARN-05 | API / Observability | Exception handler no incluía Correlation ID. Middleware añadido que propaga `X-Correlation-ID` en headers y en el cuerpo JSON de errores 500. | PASSED |
| WARN-06 | Infrastructure | Health check solo cubría DB. `SunatHealthCheck` añadido: verifica stub-mode, credenciales y conectividad HTTP al OSE (Nubefact). | PASSED |

---

## 🔴 Hallazgos Pendientes (PENDING)

### Arquitectura Backend & DDD

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
### Concurrencia y Datos

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
### Frontend Web / Mobile

| ID | Capa | Hallazgo | Riesgo | Estado |
|---|---|---|---|---|
| UI-01 | Frontend | **Tipado Débil (any):** Uso explícito de `any` en componentes clave como `TerminalVentasView.tsx` y `ReportesYGraficos.tsx`, violando la regla MUIS de tipado estricto. | Medio | PASSED |

---

## 🟡 Hallazgos Diferidos (DEFERRED)

| ID | Capa | Descripción | Estado | Fecha Límite / Sprint |
|---|---|---|---|---|
| F-10 | Infrastructure | `JwtService` inyecta `IConfiguration` directo. Funciona en Tipo 1, pero se refactorizará a `IOptions<JwtOptions>`. | DEFERRED | Sprint V2 |
| WARN-04 | Frontend / Observability | `VITE_SENTRY_DSN` vacío en `pos-pwa/.env.production`. Requiere cuenta Sentry externa — diferido hasta provisionar DSN. | DEFERRED | Cuando se configure Sentry |

---

> *Este ledger debe ser actualizado conforme se solucionen los hallazgos. Ningún PR debe ser fusionado sin actualizar el estado a PASSED.*
