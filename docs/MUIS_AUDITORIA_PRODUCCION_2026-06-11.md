# Auditoría técnica MUIS — Sistema cafedebarrio

**Fecha:** 2026-06-11 · **Auditor:** Claude Code (Arquitecto/Revisor MUIS) · **Tipología:** Tipo 2 (SaaS Escalable)
**Método:** Inspección estática exhaustiva de backend (.NET 9), frontends (dashboard, pos-pwa), persistencia, infraestructura, tests y CI. Sin ejecución de código (protocolo MUIS_DEVOPS).

---

## 1. Resumen ejecutivo

- **Estado actual:** El sistema tiene una base arquitectónica sólida (Clean Architecture, CQRS/MediatR, Result pattern, Argon2id, Serilog estructurado, Outbox SUNAT, CI con SBOM) y un historial de remediación real (39 hallazgos PASSED en el Ledger). Sin embargo, la auditoría profunda **falsifica varias entradas PASSED del Ledger** y detecta fallas funcionales y de seguridad que un despliegue real expondría en días.
- **Diagnóstico general:** Los problemas no son de "pulido" sino de **integridad de dinero y de control de acceso**: un feature completo roto (MovimientosCaja), arqueo de caja estructuralmente incorrecto, idempotencia inefectiva extremo-a-extremo (ventas duplicables), ausencia total de autorización por roles, y un guard de configuración que deja pasar la clave JWT placeholder del repo.
- **Nivel de riesgo:** **ALTO.** Riesgo directo de: forja de tokens, escalada de privilegios desde PIN de operador, ventas duplicadas, descuadre sistemático de caja, caída total del API por error transitorio de BD.
- **Conclusión preliminar:** **NO APTO PARA PRODUCCIÓN** en su estado actual. Es recuperable en 2–3 sprints disciplinados: los 7 bloqueantes están acotados y tienen corrección conocida.

---

## 2. Lectura del sistema

**Stack detectado (verificado en código, no solo en docs):**

| Capa | Tecnología | Evidencia |
|---|---|---|
| API | .NET 9, ASP.NET Core, Serilog (CompactJsonFormatter), RateLimiter nativo | `src/CafeBarrio.API/Program.cs` |
| Aplicación | MediatR (CQRS), FluentValidation, Result/Error pattern (MUIS_CORE) | `src/CafeBarrio.Application`, `src/MUIS_CORE` |
| Dominio | 18 entidades, 3 value objects, `IAuditable`/`IAggregateRoot` | `src/CafeBarrio.Domain` |
| Persistencia | EF Core 9 + SQL Server, 22 migraciones, AuditInterceptor (UtcNow ✓), UnitOfWork | `src/CafeBarrio.Infrastructure/Persistence` |
| Seguridad | JWT HMAC-SHA256 (admin 4h / operador 12h), Argon2id (128 MiB, OWASP 2026) | `Security/JwtService.cs`, `Argon2PasswordHasher.cs` |
| Integración fiscal | Outbox + BackgroundService → Nubefact OSE (boletas SUNAT), stub configurable | `BackgroundServices/SunatEmisionService.cs` |
| Frontends | dashboard (React/Vite/TS) y pos-pwa (PWA offline-first, IndexedDB + sync 30s) | `dashboard/`, `pos-pwa/` |
| Infra | Dockerfile multi-stage non-root, docker-compose (prod/dev), CI GitHub Actions (build, unit, integration, SBOM, docker-publish a ghcr.io) | raíz, `.github/workflows/ci.yml` |

**Módulos principales:** Auth (admin email/password), Operadores (PIN), Transacciones (venta + anulación), Turnos (apertura/cierre con arqueo), MovimientosCaja, Productos/Catálogos, Reportes, Configuración (IGV/IPM).

**Riesgos estructurales iniciales:** entidades anémicas con setters públicos; autorización inexistente más allá de "está autenticado"; doble mecanismo de seeding; estados SUNAT como strings mágicos; entidades aparentemente muertas (`TransaccionMayorista`, `Transporte`, `OpcionEnvio`, `UbicacionPreferencia`) que arrastran esquema sin uso.

---

## 3. Hallazgos por dimensión MUIS

> Severidades: 🔴 CRÍTICO · 🟠 ALTO · 🟡 MEDIO · 🔵 BAJO. Cada hallazgo tiene ID global (columna de la tabla §4).

### 3.1 Arquitectura

- 🟠 **[H-15] Seeding duplicado y divergente.** `Program.cs:141-234` siembra inline (Sede, TiposCliente, MetodosPago, Categorías, Cliente Mostrador, Config, admin) **y además** `Program.cs:284-287` invoca `CatalogDataSeeder` (que siembra sede, config, categorías y 16 productos). Dos fuentes de verdad para los mismos catálogos; los checks `Any()` de cada uno se pisan entre sí y el orden importa. Riesgo de drift y de estados parciales en el primer arranque.
- 🟠 **[H-16] `db.Database.Migrate()` + seeds síncronos en startup sin coordinación multi-réplica** (`Program.cs:147`). Para un Tipo 2 "SaaS Escalable" con 2+ réplicas arrancando a la vez: carrera de migración y carrera de seed (los `AnyAsync→SaveChanges` de `CatalogDataSeeder.cs` no son atómicos; `CategoriaCafe.Codigo` ni siquiera tiene índice único que actúe de red de seguridad).
- 🟡 **[H-30] Doble vía para anular**: `AnularTransaccionHandler` y `CreateAnulacionHandler` implementan la misma operación de negocio con lógica divergente (restauración de stock, validaciones). Toda corrección debe hacerse dos veces; ya divergieron.
- 🟡 **[H-31] `ITenantContext`/`IBackgroundTenantContext` definidos en MUIS_CORE y sin uso.** El "multi-sede" real se reduce a un `sedeId` por query string que ningún handler valida contra el usuario. Decidir: o se implementa tenancy o se eliminan las interfaces (scaffolding muerto confunde).
- 🟡 **[H-32] Entidades muertas con esquema vivo:** `TransaccionMayorista`, `Transporte`, `OpcionEnvio`, `UbicacionPreferencia`, `TipoCliente` no tienen handlers/endpoints; sí tienen tablas, FKs y configuración EF. Aumentan superficie de migraciones y carga cognitiva.
- ✅ Positivo: separación de capas limpia, `MUIS_CORE` reutilizable, `ValidationBehavior` centralizado, eventos de dominio vía MediatR, Outbox para SUNAT (decisión correcta, implementación con fallas — ver 3.2).

### 3.2 Backend y lógica de negocio

- 🔴 **[H-04] MovimientosCaja: feature 100 % inoperante.** `CreateMovimientoCajaCommandValidator.cs:7,12-13` acepta solo `"Entrada"/"Salida"`; `CreateMovimientoCajaHandler.cs:26-27` acepta solo `"Ingreso"/"Egreso"`. Conjuntos disjuntos: **toda petición falla** — si el cliente envía "Entrada" pasa el validator y la rechaza el handler; si envía "Ingreso" la rechaza el validator. No existe input válido. Esto además invalida cualquier intento de arqueo con movimientos manuales y demuestra ausencia de un test de integración del flujo.
- 🔴 **[H-05] Arqueo de cierre de turno estructuralmente incorrecto.** `TurnoRepository.GetTotalEfectivoByTurnoAsync` suma solo transacciones con método `EsEfectivo`; `CerrarTurnoHandler` computa `TotalEfectivoSistema = MontoApertura + esa suma`. **No resta anulaciones con devolución de efectivo ni incorpora movimientos de caja (ingresos/egresos manuales).** Resultado: faltantes/sobrantes falsos en cada cierre con anulación o egreso — el reporte de "diferencia" acusará desfalcos inexistentes u ocultará reales.
- 🔴 **[H-03] Idempotencia inefectiva extremo-a-extremo (Ledger A-01 PASSED ≠ realidad).** Backend: `CreateTransaccionCommand.cs:22` declara `Guid? IdempotencyKey = null` (opcional). Cliente: `pos-pwa/src/offline/syncService.ts:30-39` construye el request **sin la clave** (cero ocurrencias de "idempotency" en `pos-pwa/src`). En red intermitente (request llega al servidor, respuesta se pierde), el reintento del sync de 30 s **duplica la venta y duplica el descuento de stock**. La columna única `UX_Transacciones_IdempotencyKey` existe pero nunca recibe valores. Adicional: el check `GetByIdempotencyKeyAsync` en `CreateTransaccionHandler.cs:33-38` es TOCTOU (check y save separados); la violación del unique cae al `catch (ConcurrencyException)` y se reporta como "conflicto de inventario" — error engañoso.
- 🔴 **[H-06] El API completo se cae por un error transitorio de BD.** En `SunatEmisionService.cs`, el `ToListAsync` de polling (líneas 39-44) está **fuera** del try/catch; `ExecuteAsync` no envuelve `ProcesarPendientesAsync`. Una excepción (BD reiniciándose, timeout) mata el `BackgroundService` y, con el default de .NET (`BackgroundServiceExceptionBehavior.StopHost`, no hay `HostOptions` configurado en el repo — grep verificado), **detiene el host entero**: el POS deja de vender porque falló el poller de boletas.
- 🟠 **[H-12] Semántica de reintento SUNAT incoherente.** `SunatEmisionService.cs:58-60`: si el OSE responde HTTP no-2xx, `NubefactOseApiClient` devuelve `Emitida=false` sin excepción → el estado pasa a `"NoEmitida"` **terminal al primer intento** (el polling solo toma `"Pendiente"`). Los 3 reintentos y el DeadLetter solo aplican a excepciones de red. Un 500/429 transitorio de Nubefact pierde la boleta sin reintento ni DeadLetter. Además no existe endpoint admin para listar/reintentar DeadLetters (PROD-02 quedó a medias).
- 🟠 **[H-13] Anulación sin conciencia fiscal.** `AnularTransaccionHandler` no consulta `SunatEstado`: puede anularse una venta cuya boleta ya fue **emitida/aceptada por SUNAT** sin emitir nota de crédito — incumplimiento normativo. No existe flujo de NC en `ISunatService`.
- 🟠 **[H-19] Venta sin turno y con turno cerrado.** `CreateTransaccionHandler` no valida que `TurnoId` referencie un turno abierto; `TurnoId` es nullable y nada lo exige. Las ventas pueden quedar fuera de todo arqueo.
- 🟠 **[H-20] `AbrirTurnoHandler` TOCTOU:** dos peticiones concurrentes pasan el check `GetActivoBySedeAsync` y abren dos turnos (no hay índice único filtrado `(SedeId) WHERE Estado='Abierto'`).
- 🟠 **[H-21] Pago dividido sin validación:** `MontoMetodoPrimario` y `MetodoPagoSecundarioId` no tienen reglas en `CreateTransaccionCommandValidator` — puede ser negativo, mayor al total, o secundario == primario; el handler no verifica que las partes sumen el Total.
- 🟡 **[H-33] `DeleteProductoHandler:24-38`:** `catch (Exception)` genérico convierte cualquier fallo (deadlock, timeout, bug) en soft-delete silencioso reportado como éxito, sin log de la causa.
- 🟡 **[H-34] `ChangePasswordHandler:24,30`:** retorna `new Error(...)` confiando en conversión implícita y muta la entidad sin `UpdateAsync` (funciona por tracking, pero rompe el patrón del resto del código).
- 🟡 **[H-35] Eventos post-commit sin contrato:** `TransaccionCreadaEvent` se publica tras `SaveChanges`; si un handler de evento falla no hay compensación ni reintento. Hoy solo loguean — documentarlo como fire-and-forget o moverlos al Outbox antes de que alguien cuelgue lógica crítica ahí.
- 🟡 **[H-36] `ValidationBehavior` hace cast no type-safe a `TResponse.Failure(...)`** — crash en runtime si un command no retorna `Result`/`Result<T>`.

### 3.3 Seguridad

- 🔴 **[H-01] El guard de arranque acepta la clave JWT placeholder del repo.** `Program.cs:36` solo rechaza valores vacíos o que **empiecen con `"OVERRIDE_VIA_ENV_VAR"`**; el `appsettings.json` versionado trae `"Jwt:Key": "REEMPLAZAR_CON_SECRETO_MINIMO_32_CARACTERES"` (`appsettings.json:24`), que **pasa el guard**. Producción puede arrancar firmando tokens con una clave pública en GitHub → **cualquiera forja un token Admin**. PROD-01 del Ledger es parcialmente falso.
- 🔴 **[H-02] Ausencia total de autorización por roles → escalada PIN→todo.** Grep verificado: **cero** usos de `[Authorize(Roles=...)]`/policies en `src/`. Flujo: `POST /api/operadores/validar-pin` es `[AllowAnonymous]` (`OperadoresController.cs:80-91`) y entrega un JWT rol "Operador" (12 h). Como todos los endpoints solo exigen `[Authorize]`, ese token **puede crear/eliminar operadores, modificar/borrar productos, leer todos los reportes, cerrar turnos y anular ventas**. Un PIN de 6 dígitos es la única barrera hacia control administrativo del sistema. `CurrentUserService` solo expone `Email` — los handlers ni siquiera pueden comprobar rol/sede.
- 🔴 **[H-07] Rate limiting evadible por spoofing de IP.** `Program.cs:75-80` hace `KnownNetworks.Clear()` + `KnownProxies.Clear()` y `UseForwardedHeaders` está activo: **cualquier cliente** puede enviar `X-Forwarded-For` arbitrario y obtener una partición nueva del limiter por request. Consecuencias: (a) fuerza bruta de PIN (10⁶ combinaciones) sin freno real; (b) fuerza bruta de login; (c) **DoS de memoria barato**: cada `Verify` Argon2id consume 128 MiB (`Argon2PasswordHasher.cs:13`) — decenas de logins concurrentes tumban el contenedor. WARN-02 del Ledger queda debilitado.
- 🟠 **[H-22] Credenciales admin en el body de anulación.** `AnularTransaccionCommand` transporta `AdminEmail` + `AdminPassword` en JSON por cada anulación, tecleadas en el POS. Anti-patrón (resight de credenciales, logs de proxy, repetición); el endpoint `POST /api/transacciones/{id}/anular` además no tiene rate limiting. Debe sustituirse por verificación de rol del JWT (que exige resolver H-02) o un flujo de elevación puntual.
- 🟠 **[H-23] Sin lockout ni invalidación de sesiones.** `Usuario` no tiene contador de intentos ni `PasswordChangedAt`; `ChangePasswordHandler` no invalida JWTs emitidos (válidos hasta 4–12 h después de un cambio por compromiso). `Operador` tampoco tiene lockout de PIN.
- 🟠 **[H-24] JWT en `localStorage` en ambos frontends** (`dashboard/src/context/AuthProvider.tsx:7`, `pos-pwa/src/App.tsx:15-25`): exfiltrable por cualquier XSS o dependencia comprometida; sin refresh tokens. Mitigante: CSP estricta en el API, pero los frontends se sirven fuera del API.
- 🟡 **[H-37] Timing de login revela existencia de usuarios:** `LoginCommandHandler.cs:22-24` corto-circuita `Verify` (≈100+ ms de Argon2) cuando el email no existe. Verificar contra hash dummy.
- 🟡 **[H-38] Endpoints `[AllowAnonymous]` de catálogo** (`GET /api/productos`, categorías, métodos de pago, configuración/tasas, lista de operadores): exponen catálogo, stock y nombres de operadores sin autenticación. Aceptable solo si es decisión consciente para el POS pre-login; hoy expone `Costo` del producto si el DTO lo incluye — revisar proyección.
- 🟡 **[H-39] PIN: rango inconsistente** — crear exige 6-8 dígitos, `UpdateOperadorHandler` acepta 4-8 (10⁴ combinaciones).
- ✅ Positivo: Argon2id con parámetros OWASP, `FixedTimeEquals`, CORS restrictivo (G-SEC-001 cumplido), headers de seguridad, HSTS, exception handler sin stack traces y con Correlation ID, login con mensaje único.

### 3.4 Rendimiento

- 🟠 **[H-25] `ReportesRepository.GetVentasPorDia` (líneas 121-133) materializa todas las filas del rango antes del GroupBy en memoria, sin tope de rango de fechas.** El guardrail G-INF-001 justifica memory-GroupBy para *un* caso intraducible; aquí el patrón se generalizó sin límite: un año de datos = cientos de miles de filas a RAM por request de dashboard. Mitigar con: proyección agregada traducible (`GroupBy` sobre columna fecha truncada en SQL) o tope de rango (p. ej. 92 días) validado en el query.
- 🟠 **[H-26] Paginación recursiva `while(true)` sin tope ni timeout** en `dashboard/src/api/CafeBarrioApiAdapter.ts:94-106` y `pos-pwa/src/adapters/CafeBarrioPosAdapter.ts:63-77`. Una respuesta anómala del servidor produce loop infinito + OOM del navegador. Añadir `MAX_PAGES` y `AbortController`.
- 🟡 **[H-40] Argon2id 128 MiB también en `validar-pin`:** cada validación de PIN cuesta 128 MiB / ~100 ms. Con el limiter evadible (H-07) es vector DoS; aun resuelto H-07, evaluar parámetros distintos para PIN (p. ej. 64 MiB) o cache de lockout previo al hash.
- 🟡 **[H-41] `BaseRepository.GetByIdAsync` usa `FindAsync` con tracking** para lecturas puras; índice compuesto faltante orientado a `GetTopProductos` (join Detalles→Transaccion por SedeId+Fecha).
- 🔵 Sin caching de catálogos (productos/categorías se consultan por request); aceptable a esta escala, anotar para V2.

### 3.5 Calidad de código y refactorización

- 🟠 **[H-30/H-04] Duplicación divergente** (anulación por dos vías; tipos de movimiento con dos vocabularios) — es la causa raíz del bug crítico de MovimientosCaja: dos validaciones del mismo concepto en capas distintas sin un enum compartido.
- 🟡 **[H-42] Estados como strings mágicos** (`"Pendiente"/"Emitida"/"NoEmitida"/"DeadLetter"`, `"Abierto"/"Cerrado"`, `"Ingreso"/"Egreso"`, roles `"Admin"/"Operador"`) repartidos por handlers, repos y background services. Extraer a enums/constantes de dominio: elimina la clase de bug de H-04 por construcción.
- 🟡 **[H-43] Dominio anémico:** setters públicos en todas las entidades; la única invariante encapsulada es `Producto.DescontarStock`. `Transaccion.Total` se asigna desde fuera y puede divergir de `Subtotal+Impuesto`.
- 🟡 **[H-44] `CreateTransaccionHandler` concentra:** idempotencia, validación de catálogo, descuento de stock, cálculo fiscal, pago dividido y persistencia (~115 líneas). Extraer `CalculadoraFiscal` y validación de turno; facilita los tests que faltan.
- 🔵 Frontend: `as any`/`@ts-ignore` en `dashboard/vite.config.ts:13,27`; `void promesa` que traga errores de IndexedDB en `pos-pwa/src/components/SalesModule.tsx:97-100`; `parseFloat(x) || 0` convierte input inválido en S/ 0.00 (`TerminalVentasView.tsx:123`).

### 3.6 Base de datos y persistencia

- 🟠 **[H-27] `Operador.Eliminado` sin `HasQueryFilter` global** (`OperadorConfiguration.cs`): cada query debe acordarse de filtrar; el repo lo hace, pero cualquier acceso directo al DbSet fuga eliminados (reportes, joins).
- 🟠 **[H-28] `Turno` sin concurrency token:** dos cierres simultáneos del mismo turno se pisan (`MontoEfectivoCierto`/`TotalEfectivoSistema`) sin detección. `Producto` ya tiene RowVersion (D-01) — extender a `Turno`.
- 🟡 **[H-45] Unicidades de negocio sin proteger:** `CategoriaCafe.Codigo` y `Cliente.CodigoCliente` sin índice único (el seeder depende de `Codigo` único); índice de idempotencia global en vez de `(SedeId, IdempotencyKey)`.
- 🟡 **[H-46] Redondeo monetario en el cliente con float:** `pos-pwa/src/utils.ts:4-8` calcula totales con aritmética IEEE-754 para mostrar; el servidor recalcula (correcto), pero el ticket mostrado puede diferir en céntimos del comprobante SUNAT. Unificar: el cliente muestra lo que el servidor confirma, o usar enteros (céntimos).
- ✅ Positivo: `AuditInterceptor` usa `DateTime.UtcNow` (cero ocurrencias de `DateTime.Now` en `src/` — verificado); precisión decimal correcta (18,2 montos; 5,4 tasas; 10,3 stock); migraciones sin operaciones destructivas; UnitOfWork captura `DbUpdateConcurrencyException`.

### 3.7 Infraestructura, configuración y despliegue

- 🟠 **[H-11] CI puede publicar imagen con tests de integración rotos:** `ci.yml:148-151` — `docker-publish` solo `needs: build-and-test`; `integration-tests` (línea 61) no es prerequisito.
- 🟠 **[H-14] `pos-pwa/dev-dist/` versionado** (service worker generado): riesgo de servir SW viejo y de confusión de build; debe salir del repo.
- 🟡 **[H-47] Sin graceful shutdown configurado** (`HostOptions.ShutdownTimeout` ausente) ni `HEALTHCHECK` en Dockerfile/compose del API; `/health` único sin separación live/ready; sin reverse proxy TLS en `docker-compose.yml` (la API expone 8080 plano — las credenciales de login viajan en claro si no hay proxy externo).
- 🟡 **[H-48] Logs a archivo local del contenedor** (`logs/cafebarrio-*.log`): se pierden al recrear el contenedor; falta `Log.CloseAndFlush()` al apagar; sin sink centralizado (Sentry backend / Seq / App Insights).
- 🟡 **[H-49] `scripts/backup.ps1` sin `RESTORE VERIFYONLY`:** el backup corrupto se descubre en el desastre.
- 🔵 **[H-50] Higiene de repo:** `build-context.js`/`build-dashboard-context.js` con rutas absolutas Windows (`c:/Users/PC/Desktop/...`), `transcript_content.txt` vacío, `pos-pwa/extract.cjs` — sacar del repo o mover a `tools/` portable. `docker-compose.dev.yml` con `MSSQL_SA_PASSWORD` hardcodeada (aceptable en dev, parametrizar con default).
- ✅ Positivo: Dockerfile multi-stage non-root (G-DEV-002/003 cumplidos), guard `RequireConfig` (concepto correcto, implementación con el hueco H-01), templates de config (G-DEV-004), SBOM en CI, CSP/HSTS/headers.

### 3.8 Testing y confiabilidad

- 🟠 **[H-29] Los flujos que mueven dinero no tienen tests:** no existe ningún test de (a) venta concurrente del mismo producto (RowVersion en acción), (b) idempotencia con requests paralelos, (c) anulación que restaura stock, (d) cierre de turno con anulaciones y movimientos, (e) DeadLetter SUNAT. **El bug H-04 (MovimientosCaja) habría sido imposible de mergear con un único test de integración del flujo** — es la prueba de que la cobertura actual valida construcción, no comportamiento.
- 🟡 **[H-51] Integration tests comparten BD con lock global y orden-dependencia** (`IntegrationTestBase.cs:27-50`): rollback por transacción está bien, pero los seeds compartidos crean no-determinismo latente.
- 🟡 **[H-52] CI no corre lint ni tests de frontend** (solo `npm run build`); dashboard tiene 3 tests de render y pos-pwa ninguno del flujo offline/sync — justamente donde está H-03.

### 3.9 Observabilidad y operación real

- 🟠 **[H-12-bis] DeadLetters SUNAT invisibles:** no hay endpoint admin ni métrica ni alerta; una boleta perdida solo se descubre en fiscalización. Mínimo: `GET /api/admin/sunat/deadletters` + reintento manual + log ERROR al transicionar.
- 🟡 Sin `UseSerilogRequestLogging` (no hay log por request con latencia); sin métricas (`/metrics` o equivalentes); Sentry frontend diferido (WARN-04) y **ningún** error tracking en backend. El Correlation ID existe (bien) pero no hay adónde correlacionarlo.
- 🟡 Para operar en producción real faltan: alerta por DeadLetter > 0, alerta por health degradado, dashboard mínimo de: ventas/min, fallos de sync del POS, 401/429 anómalos (indicador de fuerza bruta).

### 3.10 Aristas no contempladas inicialmente

- **Zona horaria / integridad de fecha:** el POS usa `new Date()` del dispositivo (`SalesModule.tsx:186`) y los reportes diarios agrupan por `Fecha` del servidor en UTC. Perú es UTC-5: una venta a las 20:00 hora local cae en el "día siguiente" UTC → **los reportes de "ventas por día" están corridos en la cola del día**. Definir: persistir UTC + agrupar con conversión a `America/Lima` en queries de reporte.
- **Tasa IGV stale en el POS:** `pos-pwa/src/config.ts:6` lee `pos_tasaIgv` de localStorage con fallback `0.18` sin expiración; si la sede cambia configuración, los tickets mostrados quedan con tasa vieja hasta refresco manual.
- **Licencias de dependencias:** Konscious.Security.Cryptography (MIT), MediatR 12.x (Apache-2.0; nota: MediatR 13+ cambió a licencia comercial — anclar versión y documentar antes de subir mayor), DotNetEnv, Serilog — sin conflictos copyleft detectados. Recomendado: job de license-check en CI junto al SBOM ya existente.
- **Privacidad (Ley 29733 Perú):** se almacenan `NumeroDocumento`/`RazonSocial` de clientes en boletas; no hay política de retención ni cifrado en reposo más allá del de SQL Server. Documentar tratamiento y acceso.
- **Accesibilidad:** pos-pwa casi sin `aria-*` (4 ocurrencias vs 63 en dashboard); para un POS táctil interno es riesgo bajo, anotar.
- **Vendor lock-in:** acoplamiento a Nubefact está correctamente aislado tras `ISunatOseApiClient` — bien diseñado para sustitución de OSE.
- **PWA staleness:** revisar `skipWaiting/clientsClaim` del SW para que correcciones críticas (como las de esta auditoría) lleguen al POS sin intervención manual.

---

## 4. Tabla priorizada de problemas

| ID | Hallazgo | Dimensión MUIS | Severidad | Impacto | Acción correctiva | Prioridad |
|---|---|---|---|---|---|---|
| H-01 | Guard acepta Jwt:Key placeholder del repo | Seguridad | CRÍTICO | Forja de tokens Admin | Guard por denylist de valores conocidos + longitud mínima 32 + entropía; quitar placeholder de appsettings.json | P0 |
| H-02 | Sin autorización por roles; PIN→acceso total | Seguridad | CRÍTICO | Escalada de privilegios | `[Authorize(Roles="Admin")]` en endpoints administrativos; policy "Operador" limitada; extender CurrentUserService | P0 |
| H-03 | Idempotencia no enviada por POS y opcional en API | Backend/Datos | CRÍTICO | Ventas y stock duplicados | Clave obligatoria en canal POS; PWA genera UUID por venta local; manejar unique-violation como éxito idempotente | P0 |
| H-04 | MovimientosCaja: validator y handler con vocabularios disjuntos | Backend | CRÍTICO | Feature inoperante | Unificar a enum `TipoMovimientoCaja {Ingreso, Egreso}` compartido; test de integración del flujo | P0 |
| H-05 | Arqueo no incluye anulaciones ni movimientos de caja | Backend/Negocio | CRÍTICO | Descuadre sistemático de caja | Fórmula: apertura + ventas efectivo − devoluciones efectivo + ingresos − egresos; tests con escenarios | P0 |
| H-06 | Excepción del poller SUNAT detiene el host | Backend/Infra | CRÍTICO | Caída total del API | try/catch alrededor del ciclo completo + backoff; configurar `BackgroundServiceExceptionBehavior.Ignore` con log | P0 |
| H-07 | Rate limiting evadible (XFF sin KnownProxies) | Seguridad | CRÍTICO | Fuerza bruta PIN/login, DoS Argon2 | Configurar KnownProxies/Networks reales o `ForwardLimit`; en su defecto no usar XFF para particionar | P0 |
| H-12 | OSE no-2xx → NoEmitida terminal sin retry/DeadLetter | Backend/Fiscal | ALTO | Boletas perdidas | Tratar 5xx/429 como reintento; agotar MaxRetries → DeadLetter; endpoint admin DeadLetters | P1 |
| H-13 | Anulación sin nota de crédito si boleta emitida | Negocio/Fiscal | ALTO | Incumplimiento SUNAT | Bloquear anulación de emitidas hasta flujo NC, o emitir NC | P1 |
| H-22 | Credenciales admin en body de anulación; sin rate limit | Seguridad | ALTO | Exposición de credenciales | Autorizar por rol del JWT (tras H-02); rate limit al endpoint | P1 |
| H-19 | Venta sin turno abierto posible | Negocio | ALTO | Ventas fuera de arqueo | Validar turno abierto si canal POS | P1 |
| H-20 | Doble turno abierto (TOCTOU) | Datos | ALTO | Arqueo corrupto | Índice único filtrado `(SedeId) WHERE Estado='Abierto'` | P1 |
| H-21 | Pago dividido sin validación de montos | Backend | ALTO | Totales manipulables | Reglas FluentValidation + cuadre contra Total en handler | P1 |
| H-23 | Sin lockout ni invalidación de tokens | Seguridad | ALTO | Persistencia de compromiso | Lockout (5 intentos/15 min) en Usuario y Operador; `PasswordChangedAt` valida `iat` | P1 |
| H-24 | JWT en localStorage | Seguridad/Frontend | ALTO | Robo por XSS | Corto plazo: expiración corta + CSP frontend; medio: cookie httpOnly + refresh | P2 |
| H-25 | Reportes materializan rango completo en RAM | Rendimiento | ALTO | OOM/latencia con histórico | GroupBy traducible o tope de rango (≤92 días) | P1 |
| H-26 | Paginación `while(true)` sin tope | Frontend | ALTO | Loop infinito/OOM navegador | MAX_PAGES + AbortController | P1 |
| H-27 | Soft-delete sin query filter global | Datos | ALTO | Fuga de eliminados | `HasQueryFilter(o => !o.Eliminado)` | P1 |
| H-28 | Turno sin RowVersion | Datos | ALTO | Cierres concurrentes se pisan | Concurrency token + manejo en handler | P1 |
| H-29 | Cero tests de flujos de dinero | Testing | ALTO | Regresiones invisibles | Suite: concurrencia stock, idempotencia, anulación+stock, arqueo, DeadLetter | P1 |
| H-11 | docker-publish sin integration-tests | DevOps | ALTO | Imagen rota en ghcr.io | `needs: [build-and-test, integration-tests]` | P1 |
| H-14 | dev-dist versionado | DevOps | ALTO | SW viejo en producción | `git rm -r`, .gitignore | P1 |
| H-15 | Seeding duplicado inline + CatalogDataSeeder | Arquitectura | ALTO | Drift de datos semilla | Consolidar en CatalogDataSeeder único | P2 |
| H-16 | Migrate/seed sin coordinación multi-réplica | Infra | MEDIO* | Carreras en arranque escalado | Lock distribuido o job de migración separado (*ALTO si se escala) | P2 |
| H-37 | Timing attack en login | Seguridad | MEDIO | Enumeración de usuarios | Verify contra hash dummy | P2 |
| H-38 | Catálogo/operadores anónimos | Seguridad | MEDIO | Info disclosure | Revisar DTOs (no exponer Costo); decisión consciente documentada | P2 |
| H-39 | PIN 4-8 vs 6-8 inconsistente | Seguridad | MEDIO | PIN débil por update | Unificar 6-8 con validator compartido | P2 |
| H-30 | Doble vía de anulación divergente | Calidad | MEDIO | Bugs duplicados | Consolidar en un solo handler | P2 |
| H-42 | Estados como strings mágicos | Calidad | MEDIO | Clase de bug H-04 | Enums de dominio | P2 |
| H-45 | Unicidades sin índice (Codigo, idempotencia por sede) | Datos | MEDIO | Duplicados de catálogo | Índices únicos | P2 |
| H-46 | Redondeo float en cliente | Datos/Frontend | MEDIO | Ticket ≠ comprobante por céntimos | Mostrar montos confirmados por servidor | P2 |
| H-47 | Sin graceful shutdown/HEALTHCHECK/live-ready/TLS proxy | Infra | MEDIO | Operación frágil | HostOptions 30s, HEALTHCHECK, /health/live+ready, proxy TLS | P2 |
| H-48 | Logs locales sin centralizar, sin flush | Observabilidad | MEDIO | Diagnóstico imposible post-mortem | Sink centralizado + CloseAndFlush + RequestLogging | P2 |
| ZH-01 | Reportes por día en UTC (Perú UTC-5) | Aristas | MEDIO | Ventas asignadas al día equivocado | Agrupar con conversión a America/Lima | P2 |
| H-33/34/35/36 | Catch genérico, retornos implícitos, eventos sin contrato, cast behavior | Calidad | MEDIO | Robustez | Refactors puntuales | P3 |
| H-49/50/51/52 | Backup sin verify, higiene repo, tests no deterministas, lint CI | DevOps/Testing | BAJO | Mantenibilidad | Acciones puntuales | P3 |
| H-40/41/43/44 | Argon2 en PIN, tracking en GetById, dominio anémico, handler largo | Rendimiento/Calidad | BAJO | Eficiencia/evolución | Refactors V2 | P3 |

---

## 5. Refactorizaciones recomendadas (top 6, con rationale)

1. **Enum compartido `TipoMovimientoCaja` + enums de estados SUNAT/Turno** — *Problema:* el mismo concepto se valida con strings distintos en capas distintas (causa directa del bug crítico H-04). *Riesgo de no hacerlo:* cualquier feature nuevo repite la clase de bug. *Enfoque:* enums en `CafeBarrio.Domain`, conversión EF a string, validators y handlers consumen el mismo tipo. *Beneficio:* elimina la categoría de error por construcción; el compilador se vuelve el test.
2. **Idempotencia transaccional única** — *Problema:* check-then-insert TOCTOU + clave opcional. *Enfoque:* clave obligatoria para canal POS; capturar `SqlException` 2601/2627 sobre `UX_Transacciones_IdempotencyKey` y responder el `TransaccionId` existente (éxito idempotente), distinto del conflicto de stock. *Beneficio:* reintentos seguros por diseño, sin SERIALIZABLE.
3. **Servicio de arqueo (`CalculadoraArqueo`)** — *Problema:* la fórmula de efectivo esperado vive incompleta dentro de un repo. *Enfoque:* servicio de dominio puro (testeable sin BD) que recibe ventas efectivo, devoluciones, ingresos y egresos; `CerrarTurnoHandler` lo orquesta. *Beneficio:* fórmula correcta, visible y testeada; el cierre deja de mentir.
4. **Autorización como policy transversal** — *Problema:* la autorización "se haría" en cada handler y no se hace en ninguno. *Enfoque:* policies `AdminOnly`/`OperadorOrAdmin` en Program.cs + atributos por endpoint + `ICurrentUserService` ampliado (UserId, Rol); eliminar `AdminEmail/AdminPassword` del command de anulación. *Beneficio:* superficie de ataque reducida en un solo cambio coherente.
5. **Pipeline SUNAT resiliente** — *Problema:* poller frágil (mata el host) y semántica de retry incoherente. *Enfoque:* envolver ciclo en try/catch con backoff; clasificar respuestas OSE (4xx definitivo → NoEmitida; 5xx/429/timeout → retry → DeadLetter); endpoint admin de DeadLetters. *Beneficio:* el subsistema fiscal falla aislado y recuperable, nunca tumba ventas.
6. **Consolidar seeding en `CatalogDataSeeder`** — *Problema:* dos seeders compiten en startup. *Enfoque:* mover Sede/MetodosPago/TiposCliente/Cliente Mostrador/Config/admin al seeder (ya idempotente), borrar el bloque inline de Program.cs, y proteger con índices únicos los códigos de negocio. *Beneficio:* arranque determinista y Program.cs legible.

---

## 6. Checklist de producción

**Configuración**
- [ ] Jwt:Key sin placeholder posible (guard reforzado + sin valor en appsettings.json) — **bloqueado por H-01**
- [x] Validación fail-fast de ConnectionString y CORS en non-Development
- [x] Templates de configuración versionados (G-DEV-004)
- [ ] `Seed:AdminPassword` documentado como requisito de primer arranque

**Seguridad**
- [ ] Autorización por roles en todos los endpoints administrativos — **bloqueado por H-02**
- [ ] Rate limiting no evadible (proxies conocidos) — **bloqueado por H-07**
- [ ] Lockout de login y PIN — **H-23**
- [x] Hashing Argon2id, CORS explícito, headers de seguridad, HSTS, sin stack traces
- [ ] TLS extremo a extremo (proxy inverso o Kestrel HTTPS en compose)

**Datos**
- [ ] Idempotencia efectiva en el canal POS — **bloqueado por H-03**
- [ ] Arqueo correcto (anulaciones + movimientos) — **bloqueado por H-04/H-05**
- [ ] Turno con concurrency token y unicidad de turno abierto — **H-20/H-28**
- [x] Migraciones EF aplicadas con `Migrate()`; auditoría UTC; RowVersion en Producto
- [ ] Backup con `RESTORE VERIFYONLY` y restore ensayado

**Observabilidad**
- [x] Logs estructurados JSON con Correlation ID
- [ ] Logs/errores centralizados fuera del contenedor — **H-48**
- [ ] DeadLetters SUNAT visibles y alertables — **H-12**
- [ ] /health/live + /health/ready separados; HEALTHCHECK en imagen

**Rendimiento**
- [ ] Reportes con tope de rango o agregación en SQL — **H-25**
- [ ] Paginación frontend con tope — **H-26**
- [x] Índices de rendimiento base (migraciones Ref07); AsNoTracking mayoritario

**Testing**
- [ ] Tests de dinero: concurrencia de stock, idempotencia, anulación, arqueo — **H-29**
- [x] Unit + integration en CI con SQL real
- [ ] CI bloquea publicación si integración falla — **H-11**
- [ ] Lint + tests de frontend en CI

**Despliegue / Operación**
- [x] Imagen multi-stage non-root; SBOM en CI; ghcr.io
- [ ] Graceful shutdown (HostOptions); poller SUNAT no puede tumbar el host — **H-06**
- [ ] Estrategia de migración para >1 réplica — **H-16**
- [ ] Runbook mínimo: restaurar backup, reintentar DeadLetters, rotar Jwt:Key

**Resultado: 11/30 ítems cumplidos. No se cumple el umbral de producción.**

---

## 7. Riesgos residuales (aun después de aplicar todo lo anterior)

1. **JWT en localStorage** seguirá siendo exfiltrable por XSS hasta migrar a cookies httpOnly + refresh tokens (cambio de arquitectura de sesión, P2/V2).
2. **Single point of failure físico:** despliegue on-premise con SQL Server local; un fallo de disco sin backup verificado reciente pierde ventas. Mitigable, no eliminable, sin réplica.
3. **Dependencia del reloj y la red del local:** el POS offline confía en el dispositivo; con idempotencia correcta no habrá duplicados, pero la *latencia fiscal* (boletas pendientes mientras no hay red) es inherente al diseño y debe ser aceptada por negocio con un límite documentado.
4. **Nota de crédito SUNAT:** aun bloqueando la anulación de boletas emitidas (H-13), el flujo completo de NC es trabajo nuevo; hasta entonces habrá anulaciones que requieren gestión manual en Nubefact.
5. **Modelo de confianza del PIN:** un PIN de 6 dígitos con lockout es razonable para operadores en LAN del local, pero si el API se expone a internet pública el factor único seguirá siendo débil; considerar binding por dispositivo en V2.
6. **Dominio anémico:** mientras las invariantes (Total = Subtotal + Impuesto, stock ≥ 0 salvo override) no vivan en las entidades, cada handler nuevo puede reintroducir inconsistencias.

---

## 8. Veredicto final

# 🔴 NO APTO PARA PRODUCCIÓN

**No es posible asegurar producción con rigor todavía por estas razones:**

1. **Seguridad rota en dos puntos estructurales:** la clave JWT placeholder del repositorio pasa el guard de arranque (H-01) y no existe autorización por roles — un PIN de 6 dígitos, atacable porque el rate limiting es evadible por spoofing de cabeceras (H-07), otorga control administrativo total del sistema (H-02).
2. **El dinero no cuadra por diseño:** MovimientosCaja no admite ningún input válido (H-04), el arqueo de cierre omite anulaciones y movimientos (H-05) y las ventas del POS son duplicables en redes intermitentes porque la idempotencia nunca se envía (H-03). Estos tres invalidan la función primaria de un POS: custodiar la caja.
3. **Estabilidad:** un error transitorio de BD en el poller SUNAT detiene el host completo (H-06); un 5xx del OSE pierde boletas sin reintento (H-12).
4. **Evidencia insuficiente:** no existe ni un test de los flujos de dinero (H-29); esta auditoría es estática y no sustituye una validación dinámica — queda pendiente probar: arranque limpio multi-entorno, carga concurrente de ventas, restore de backup y el ciclo completo OSE real.

**Cambios obligatorios antes del deploy (gate P0):** H-01, H-02, H-03, H-04, H-05, H-06, H-07 — más la suite de tests que los demuestre (H-29 parcial) y la corrección del pipeline (H-11). Con el gate P0 cerrado y validado, el sistema pasaría a **APTO CON CAMBIOS OBLIGATORIOS** (P1 en paralelo a un piloto controlado en una sola sede con supervisión de arqueos).

**Nota de gobernanza:** deben corregirse en el `MUIS_EVALUATION_LEDGER.md` las entradas **A-01** (idempotencia: implementada en BD, inefectiva extremo-a-extremo), **WARN-02** (rate limiting: presente pero evadible) y **PROD-01/PROD-02** (guard con bypass; retry SUNAT incompleto). Reabrirlas con los IDs de esta auditoría.

---

## 9. Plan de ejecución por otro agente

> Protocolo MUIS_DEVOPS: cada TASK de esta sección se emite como **Task Bundle B** individual (template en `MUIS_DEVOPS/bundles/TASK_BUNDLE_TEMPLATE.md`) cuando se vaya a ejecutar. Cada tarea cierra con su **Validation Report** obligatorio. Pre-flight de guardrails aplicable: G-SEC-001 (CORS), G-INF-001 (GroupBy), G-INF-003 (seeder), G-DEV-001 (sobreescribir, no append).

### 9.1 Acciones inmediatas (gate P0 — bloqueantes de deploy)

**TASK-P0-01 — Cerrar bypass del guard Jwt:Key** `[H-01]`
- Archivo: `src/CafeBarrio.API/Program.cs:31-45` y `src/CafeBarrio.API/appsettings.json:24`.
- Cambios: (1) eliminar el valor de `Jwt:Key` en `appsettings.json` (dejar `""`); (2) en `RequireConfig`, además del prefijo `OVERRIDE_VIA_ENV_VAR`, rechazar una denylist de placeholders conocidos (`REEMPLAZAR_`, `DEV_JWT_KEY`, `AQUI_`) y exigir longitud ≥ 32; (3) aplicar el mismo guard a `Seed:AdminPassword` cuando no existan usuarios.
- Aceptación: arrancar en `Production` con el appsettings actual del repo **lanza** `InvalidOperationException`; con clave válida de 32+ chars arranca.

**TASK-P0-02 — Autorización por roles** `[H-02][H-22]`
- Archivos: `Program.cs` (policies), todos los controllers, `Security/CurrentUserService.cs`, `Common/Interfaces/ICurrentUserService.cs`, `AnularTransaccionCommand/Handler`.
- Cambios: (1) policies `AdminOnly` (rol Admin) y `PosOperador` (Operador o Admin); (2) `[Authorize(Policy="AdminOnly")]` en: Operadores POST/PUT/DELETE, Productos POST/PUT/DELETE, Reportes (todos), Turnos cerrar, Anulaciones, Transacciones GET lista/detalle; `PosOperador` en Transacciones POST, Turnos abrir/activo, MovimientosCaja; (3) ampliar `ICurrentUserService` con `UserId`, `Rol`; (4) `AnularTransaccionCommand`: eliminar `AdminEmail/AdminPassword`, autorizar por policy y registrar `OperadorSolicitanteId` + usuario del token en la anulación; añadir `[EnableRateLimiting("api-write-policy")]` al endpoint.
- Aceptación: token Operador recibe 403 en endpoints Admin (test de integración por endpoint); anulación funciona solo con token Admin.

**TASK-P0-03 — Idempotencia extremo a extremo** `[H-03]`
- Archivos: `pos-pwa/src/offline/pendingStore.ts`, `syncService.ts`, `types.ts`; backend `CreateTransaccionHandler.cs`, `CreateTransaccionCommandValidator.cs`.
- Cambios: (1) PWA: generar `idempotencyKey: crypto.randomUUID()` al **crear** la venta local (persistida en IndexedDB) y enviarla en el request — también en la venta online directa; (2) backend: `IdempotencyKey` obligatoria cuando `Canal == "POS"` (validator); (3) en el handler, capturar la violación del índice único `UX_Transacciones_IdempotencyKey` (SqlException 2601/2627 vía `DbUpdateException`) y devolver `Success(existente.TransaccionId)`, separada del conflicto de stock.
- Aceptación: test de integración con 2 requests paralelos misma clave → una sola fila, ambos responden el mismo ID; reintento de sync no duplica stock.

**TASK-P0-04 — Reparar MovimientosCaja** `[H-04][H-42 parcial]`
- Archivos: nuevo `src/CafeBarrio.Domain/Enums/TipoMovimientoCaja.cs` (`Ingreso`, `Egreso`), `CreateMovimientoCajaCommandValidator.cs`, `CreateMovimientoCajaHandler.cs`, frontend que consuma el endpoint.
- Cambios: vocabulario único "Ingreso"/"Egreso" (coincide con handler y con datos existentes); validator y handler consumen la misma fuente; test de integración del flujo completo (crear ingreso, crear egreso, listar por turno).
- Aceptación: `POST /api/movimientos-caja` con "Ingreso" responde 201 y persiste; "Entrada" responde 400 desde el validator con mensaje correcto.

**TASK-P0-05 — Arqueo correcto de cierre de turno** `[H-05]`
- Archivos: `Features/Turnos/Commands/CerrarTurno/CerrarTurnoHandler.cs`, `TurnoRepository.cs`, nuevo servicio de dominio `CalculadoraArqueo`.
- Cambios: `TotalEfectivoSistema = MontoApertura + ventasEfectivo − devolucionesEfectivoAnulaciones + ingresosCaja − egresosCaja` (solo anulaciones con devolución en efectivo y movimientos del turno). Lógica en servicio puro testeable; repo aporta las 4 sumas en queries agregadas SQL.
- Aceptación: tests unitarios con matriz de escenarios (sin movimientos; con anulación efectivo; con anulación tarjeta — no resta; con ingreso+egreso) y un test de integración.

**TASK-P0-06 — Poller SUNAT no puede tumbar el host + retry coherente** `[H-06][H-12]`
- Archivos: `BackgroundServices/SunatEmisionService.cs`, `External/Sunat/NubefactOseApiClient.cs`, `Program.cs`.
- Cambios: (1) envolver el cuerpo de `ProcesarPendientesAsync` (incluida la query) en try/catch con log y backoff (30s→5min progresivo en fallos consecutivos); (2) `services.Configure<HostOptions>(o => { o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore; o.ShutdownTimeout = TimeSpan.FromSeconds(30); })`; (3) clasificar respuesta OSE: 4xx ≠ 429 → `NoEmitida` definitivo con error; 5xx/429/timeout → mantener `Pendiente` e incrementar intentos → `DeadLetter` al agotar; (4) endpoint `GET /api/admin/sunat/deadletters` + `POST /api/admin/sunat/deadletters/{id}/reintentar` (policy AdminOnly, reusa TASK-P0-02).
- Aceptación: test con `ISunatService` que lanza excepción → el host sigue respondiendo `/health`; test de transición a DeadLetter en 3 intentos por 5xx.

**TASK-P0-07 — Rate limiting no evadible** `[H-07]`
- Archivo: `Program.cs:75-80, 82-126`.
- Cambios: (1) si hay proxy conocido: configurar `KnownProxies`/`KnownNetworks` con sus IPs reales y `ForwardLimit = 1`; (2) si el despliegue actual es sin proxy: **eliminar** `UseForwardedHeaders` y los `Clear()`; (3) documentar la decisión en un ADR nuevo (precedente: F-04).
- Aceptación: con `X-Forwarded-For` falsificado en requests directos, el limiter agota por IP de conexión real (test de integración con 21 logins).

**TASK-P0-08 — CI bloquea publicación sin integración** `[H-11]`
- Archivo: `.github/workflows/ci.yml:148-151` → `needs: [build-and-test, integration-tests]`.
- Aceptación: en un PR con test de integración rojo, `docker-publish` no se ejecuta.

### 9.2 Acciones de alta prioridad (P1 — antes del piloto)

- **TASK-P1-01** `[H-19][H-20]` Validar turno abierto en `CreateTransaccionHandler` para canal POS; migración con índice único filtrado `IX_Turnos_SedeId_Abierto ON Turnos(sede_id) WHERE estado='Abierto'`; manejar la unique-violation en `AbrirTurnoHandler` como "Turno.YaAbierto".
- **TASK-P1-02** `[H-21]` Validator de pago dividido: `MontoMetodoPrimario > 0` y `< Total` cuando hay secundario; `MetodoPagoSecundarioId != MetodoPagoId`; en handler, cuadre `montoPrimario + montoSecundario == Total`.
- **TASK-P1-03** `[H-23][H-39]` Lockout: campos `IntentosFallidos`/`BloqueadoHasta` en `Usuario` y `Operador` (migración); 5 fallos → 15 min; unificar PIN 6-8 en create y update con validator compartido; `PasswordChangedAt` en Usuario y rechazo de JWT con `iat` anterior (claim check en `OnTokenValidated`).
- **TASK-P1-04** `[H-13]` Bloquear anulación cuando `SunatEstado == "Emitida"` con error explícito "requiere nota de crédito — gestionar en OSE"; registrar en el Ledger el flujo NC como deuda V2.
- **TASK-P1-05** `[H-27][H-28][H-45]` Migración: `HasQueryFilter` para `Operador.Eliminado`; RowVersion en `Turno` (+ manejo de `DbUpdateConcurrencyException` en cierre); índices únicos `CategoriaCafe.Codigo` y `(SedeId, IdempotencyKey)`.
- **TASK-P1-06** `[H-25][H-26]` Reportes: tope de rango 92 días en `PeriodoHelper`/validators y agregación en SQL donde EF Core 9 lo traduzca (respetar G-INF-001: solo memory-GroupBy donde esté documentado intraducible); frontend: `MAX_PAGES=50` + `AbortController` en ambos adapters.
- **TASK-P1-07** `[H-29]` Suite de tests de dinero (integración, SQL real): venta concurrente mismo producto (no oversell), idempotencia paralela, anulación restaura stock exacto, arqueo con matriz de escenarios, DeadLetter SUNAT. Es el criterio de aceptación transversal del gate P0.
- **TASK-P1-08** `[H-14][H-50]` Higiene: `git rm -r pos-pwa/dev-dist`, eliminar `build-context.js`, `build-dashboard-context.js`, `transcript_content.txt`, `pos-pwa/extract.cjs`, `dashboard/extract-dashboard.cjs`; actualizar `.gitignore`.
- **TASK-P1-09** `[H-15]` Consolidar seeding: mover el bloque inline de `Program.cs:141-234` a `CatalogDataSeeder` (respetando G-INF-003) con creación de admin condicionada a `Seed:AdminPassword`.

### 9.3 Acciones de mediana prioridad (P2 — durante el piloto)

- **TASK-P2-01** `[H-47]` `HEALTHCHECK` en Dockerfile (curl a `/health/live`); separar `/health/live` (sin checks externos) y `/health/ready` (DB + SUNAT); compose de producción con proxy TLS (Caddy o nginx) delante del API.
- **TASK-P2-02** `[H-48]` `UseSerilogRequestLogging()`, `Log.CloseAndFlush()` en shutdown, sink centralizado (Seq autohospedado encaja con on-premise; o Sentry backend, que además cierra WARN-04 con un solo proveedor).
- **TASK-P2-03** `[ZH-01]` Reportes por día en hora de Perú: conversión `AT TIME ZONE 'SA Pacific Standard Time'` (o cómputo del rango UTC equivalente al día local) en queries de `ReportesRepository`; test con venta a las 23:30 Lima.
- **TASK-P2-04** `[H-37][H-38]` Verify contra hash dummy en login; revisar `ProductoDto` anónimo (excluir `Costo` y stock si no es necesario para el POS pre-login); decidir y documentar en ADR qué endpoints permanecen anónimos.
- **TASK-P2-05** `[H-30][H-42]` Consolidar anulación en un solo handler; enums de dominio para estados SUNAT y Turno (migración de datos no necesaria si se mantiene el mapeo string actual).
- **TASK-P2-06** `[H-33][H-34][H-36]` `DeleteProductoHandler`: catch específico de FK (SqlException 547) con respuesta explícita "desactivado por referencias"; `ChangePasswordHandler` con `Result.Failure` explícito; constraint documentado en `ValidationBehavior`.
- **TASK-P2-07** `[H-46]` POS muestra los montos confirmados por el servidor en el ticket (la respuesta de `POST /api/transacciones` debe devolver subtotal/impuesto/total); eliminar cálculo float local como fuente de verdad; refrescar `pos_tasaIgv` con TTL 24 h.
- **TASK-P2-08** `[H-16]` Estrategia de migración multi-réplica: job/contenedor `migrate` separado en compose (o lock con `sp_getapplock`) y réplicas del API sin `Migrate()`.
- **TASK-P2-09** `[H-49]` `backup.ps1`: `RESTORE VERIFYONLY` + checksum + alerta si falla; documentar runbook de restore y ensayarlo una vez.

### 9.4 Acciones recomendadas no bloqueantes

- **TASK-P3-01** `[H-24]` Migrar sesión a cookie httpOnly + refresh token con rotación (cambio coordinado backend+frontends). Crear ADR.
- **TASK-P3-02** `[H-43][H-44]` Encapsular invariantes en entidades (`Transaccion.CalcularTotales`, factory de `Turno`); extraer `CalculadoraFiscal` de `CreateTransaccionHandler`.
- **TASK-P3-03** `[H-52]` CI: `npm run lint` + `vitest` para dashboard y pos-pwa; tests del flujo offline/sync del POS (mock de IndexedDB).
- **TASK-P3-04** `[H-31][H-32]` Decidir destino de `ITenantContext` y entidades muertas (`TransaccionMayorista`, `Transporte`, `OpcionEnvio`, `UbicacionPreferencia`): implementar o eliminar con migración. Crear ADR.
- **TASK-P3-05** `[H-40][H-41]` Perfil Argon2 diferenciado para PIN (64 MiB) tras implementar lockout; `AsNoTracking` en lecturas de `BaseRepository`.
- **TASK-P3-06** Accesibilidad del POS (aria-labels, `aria-live` en carrito); license-check en CI; documentar política de datos personales (Ley 29733).
- **TASK-P3-07** Gobernanza: actualizar `MUIS_EVALUATION_LEDGER.md` (reabrir A-01, WARN-02, PROD-01, PROD-02 con referencia a esta auditoría); crear guardrails candidatos: *G-APP-001 "validator y handler deben consumir el mismo enum de dominio"* (lección de H-04, scope MUIS-CORE), *G-SEC-002 "todo endpoint nuevo declara policy explícita"* (lección de H-02, scope MUIS-CORE), *G-INF-004 "BackgroundService: ciclo completo en try/catch + HostOptions.Ignore"* (lección de H-06, scope TIPO-2).

---

*Auditoría estática. El veredicto debe revalidarse con evidencia dinámica (suite TASK-P1-07 en verde + piloto supervisado) antes de cualquier promoción a producción.*
