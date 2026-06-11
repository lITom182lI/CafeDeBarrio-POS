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
| F-05 | Infrastructure | `ReportesRepository` cargaba todas las columnas antes de agrupar. Corregido: `Where` + `Select(Fecha, Total)` en SQL, luego `GroupBy` en memoria. EF Core 9 no puede traducir `GroupBy(t => t.Fecha.Date)` con filtro de navegación a SQL. | PASSED |
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
| PROD-05 | Infrastructure / DevOps | Sin datos de catálogo inicial: sistema arrancaba vacío, inutilizable sin carga manual. `CatalogDataSeeder` idempotente añadido: siembra Sede 1, ConfiguracionNegocio, 3 categorías y 16 productos al primer arranque. | PASSED |
| DEVX-01 | DevOps / DX | Sin proceso estandarizado de onboarding para nuevos desarrolladores. `appsettings.Development.json` gitignoreado sin template de referencia. Añadidos `docker-compose.dev.yml`, `appsettings.Development.template.json` y `scripts/dev-setup.ps1`. Un solo comando post-clonado levanta SQL Server, crea configs y aplica migraciones. | PASSED |

### Auditoría de Seguridad Fable 5 — Junio 2026

| ID | Capa | Descripción | Estado |
|---|---|---|---|
| H-01 | API / Security | JWT guard sólo bloqueaba un placeholder. Expandido a 5 placeholders conocidos + mínimo 32 chars. | PASSED |
| H-02 | PWA / Security | Backdoor `offline_generic_token` hardcodeado en `App.tsx`/`LoginScreen.tsx`. Eliminado; reemplazado por `tryRestoreOfflineSession()` con validación de expiración JWT. | PASSED |
| H-03 | Application | `IdempotencyKey` opcional en `CreateTransaccionCommand`; PWA nunca la enviaba. Ahora es `required`, tabla `IdempotencyRecords` con índice único, UUID generado con `crypto.randomUUID()` en IndexedDB. | PASSED |
| H-04 | Application | `CreateMovimientoCajaValidator` usaba `"Entrada"/"Salida"`, handler usaba `"Ingreso"/"Egreso"` — ningún input era válido. Unificado con constantes `TipoMovimiento` de dominio. | PASSED |
| H-05 | Application / Domain | Fórmula de arqueo incorrecta (`apertura + ventas`). Corregido: `apertura + ventas - anulaciones + entradas - salidas`. Encapsulado en `ResumenEfectivoDto`. Migración `AddTurnoArqueoDesglose`. | PASSED |
| H-06 | Infrastructure | `SunatEmisionService.ExecuteAsync` sin `try/catch` global — excepción no manejada mataba el host. Envuelto con `try/catch` + `OperationCanceledException` + `ContinueWith` en `Task.Delay`. | PASSED |
| NEW-02 | API / Security | `GET /api/operadores` sin `[Authorize]` y sin filtro soft-delete. Aplicado `[Authorize(Roles=Admin)]` y `.Where(o => !o.Eliminado)` en repositorio. Nuevo endpoint público `GET /api/operadores/activos` para login screen POS. | PASSED |
| NEW-03 | Application / Security | RBAC ausente en todos los controllers. Aplicado `Roles.Admin` / `Roles.Operador` vía constantes de dominio. | PASSED |
| NEW-04 | Application | Descuento de stock no atómico (foreach con SaveChanges por ítem). Refactorizado con `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` en `IUnitOfWork`. Validación de stock completa previa al descuento. | PASSED |
| NEW-05 | API / Security | `ForwardedHeaders` sin lista de proxies confiables — bypass de rate limiting vía IP spoofing. `TrustedProxyIPs` leída desde configuración. | PASSED |
| NEW-07 | API / Security | Parámetro `periodo` en reportes sin whitelist. Creado `PeriodoReporte` con `HashSet<string>` validado en 7 endpoints. Handler usa `switch` sobre constantes, no SQL dinámico. | PASSED |
| NEW-09 | Domain / Security | Sin `FailedLoginAttempts`/`IsLocked` en `Usuario`. Implementado lockout de 5 intentos / 15 min con reset automático en login exitoso. Migración `AddUsuarioLockout`. | PASSED |
| NEW-10 | Domain / Security | Sin `SecurityStamp` en `Usuario` — JWT no se invalidaba al cambiar password. Añadido `SecurityStamp` (GUID), incluido en claims JWT, validado en `OnTokenValidated` contra BD. Regenerado en `ChangePasswordHandler`. Migración `AddUsuarioSecurityStamp`. | PASSED |
| P1-01 | Infrastructure / DB | Sin unique filtered index en `Turnos` para garantizar un solo turno abierto por sede. Añadido `UX_Turnos_SedeId_Abierto` con filtro `[Estado] = 'Abierto'`. Handler ya validaba en código; índice actúa como safety net ante race conditions. | PASSED |
| DEVX-02 | DevOps | `setup.ps1` y `.env.example` añadidos. Flujo completo en nueva PC: `git clone → .\setup.ps1 → docker compose up -d`. Auto-migración ya existente en `Program.cs`. | PASSED |
| DEVX-03 | DevOps / CI | `docker-publish` job no corría por CI rojo. Tests unitarios e integración corregidos (40/40 + 8/8). CI verde, imagen publicada a `ghcr.io`. | PASSED |

---

## 🔴 Hallazgos Pendientes (PENDING)

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

---

## Integración con sistema de memoria arquitectónica

Las entradas de este Ledger con patrones recurrentes o decisiones no obvias tienen entradas correspondientes en:
- **Guardrails** (`docs/guardrails/`): qué no hacer y por qué — consultar antes de emitir Bundle B
- **Decisiones** (`docs/adr/`): rationale completo con alternativas evaluadas

Ver `docs/MUIS_GUARDRAILS.md` para el índice maestro y el protocolo de carga para IA.

---

> *Este ledger debe ser actualizado conforme se solucionen los hallazgos. Ningún PR debe ser fusionado sin actualizar el estado a PASSED.*
