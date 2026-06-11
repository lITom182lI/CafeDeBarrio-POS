# MUIS GUARDRAILS — CafeDeBarrio-POS
**Versión:** 1.0.0 | **Actualizado:** 2026-06-11 | **Tipología:** Tipo 2 (SaaS Escalable)

Índice maestro del sistema de memoria arquitectónica persistente.
Cargado en cada sesión de IA vía SESSION_BUNDLE_A. No contiene detalles — apunta a entradas individuales.

## Protocolo de carga para IA

| Situación | Qué cargar |
|---|---|
| Inicio de sesión | Solo este archivo (siempre) |
| Antes de emitir Bundle B | `docs/guardrails/INDEX.md` filtrado por capa afectada |
| Colisión potencial detectada | Entrada individual `docs/guardrails/G-[CAPA]-[NNN]-*.md` |
| Decisión arquitectónica en juego | `docs/adr/ADR-[NNN]-*.md` completo |

## Registro de Guardrails

| ID | Tipo | Capa | Trigger keywords | Scope |
|---|---|---|---|---|
| [G-INF-001](guardrails/G-INF-001-ef-core-groupby-memory.md) | anti-pattern | infrastructure | EF Core, GroupBy, fecha, Date | PROYECTO |
| [G-INF-002](guardrails/G-INF-002-ihttpclientfactory-unconditional.md) | anti-pattern | infrastructure | IHttpClientFactory, AddHttpClient, condicional | PROYECTO |
| [G-INF-003](guardrails/G-INF-003-seeder-over-hasdata.md) | validated-decision | infrastructure | HasData, seed, catálogo, productos | TIPO-2 |
| [G-SEC-001](guardrails/G-SEC-001-cors-explicit-methods-headers.md) | validated-decision | security | CORS, AllowAny, headers, methods | MUIS-CORE |
| [G-DEV-001](guardrails/G-DEV-001-antigravity-append-sobreescribir.md) | anti-pattern | devops | Antigravity, reemplazar, archivo, append | MUIS-CORE |
| [G-DEV-002](guardrails/G-DEV-002-docker-tag-lowercase.md) | anti-pattern | devops | Docker, tag, imagen, uppercase, ghcr | MUIS-CORE |
| [G-DEV-003](guardrails/G-DEV-003-dockerfile-restore-csproj-not-sln.md) | anti-pattern | devops | Dockerfile, dotnet restore, .sln, build context | TIPO-2 |
| [G-DEV-004](guardrails/G-DEV-004-appsettings-dev-template-pattern.md) | validated-decision | devops | appsettings.Development, gitignore, template, onboarding | TIPO-2 |

## Registro de Decisiones (ADR)

| ID | Título | Estado | Guardrail |
|---|---|---|---|
| [ADR-001](adr/ADR-001-tipologia-muis.md) | Clasificación MUIS — Tipo 1 inicial | accepted | — |
| [ADR-002](adr/ADR-002-offline-first-pos.md) | Arquitectura Offline-First — Cliente POS | accepted | — |
| [ADR-003](adr/ADR-003-memory-groupby-efcore9.md) | Memory GroupBy sobre SQL GroupBy — EF Core 9 | accepted | G-INF-001 |
| [ADR-004](adr/ADR-004-ihttpclientfactory-unconditional.md) | IHttpClientFactory registro incondicional en Program.cs | accepted | G-INF-002 |
| [ADR-005](adr/ADR-005-catalog-seeder-over-hasdata.md) | CatalogDataSeeder sobre HasData() para datos de negocio | accepted | G-INF-003 |
| [ADR-006](adr/ADR-006-docker-dev-sqlserver.md) | SQL Server dev en Docker sobre dependencia SQLEXPRESS local | accepted | G-DEV-004 |

## Protocolo de actualización

**Crear guardrail:** Cuando un Validation Report resuelve un patrón recurrente o decisión no obvia.
**Crear ADR:** Cuando una decisión tiene alternativas evaluadas que alguien podría "mejorar" sin contexto.
**Templates:** `docs/guardrails/_TEMPLATE.md` · `docs/adr/_TEMPLATE.md`
**Promoción:** Scope `MUIS-CORE` → notificar para promoción al estándar central del ecosistema.
