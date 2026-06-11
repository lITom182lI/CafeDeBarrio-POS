# Decisiones Arquitectónicas (ADR) — Índice

| ID | Título | Estado | Fecha | Guardrail |
|---|---|---|---|---|
| [ADR-001](ADR-001-tipologia-muis.md) | Clasificación MUIS — Tipo 1 inicial | accepted | 2026-06-05 | — |
| [ADR-002](ADR-002-offline-first-pos.md) | Arquitectura Offline-First — Cliente POS | accepted | 2026-06-05 | — |
| [ADR-003](ADR-003-memory-groupby-efcore9.md) | Memory GroupBy sobre SQL GroupBy — EF Core 9 | accepted | 2026-06-11 | G-INF-001 |
| [ADR-004](ADR-004-ihttpclientfactory-unconditional.md) | IHttpClientFactory registro incondicional en Program.cs | accepted | 2026-06-11 | G-INF-002 |
| [ADR-005](ADR-005-catalog-seeder-over-hasdata.md) | CatalogDataSeeder sobre HasData() para datos de negocio | accepted | 2026-06-11 | G-INF-003 |
| [ADR-006](ADR-006-docker-dev-sqlserver.md) | SQL Server dev en Docker sobre dependencia SQLEXPRESS local | accepted | 2026-06-11 | G-DEV-004 |

## Estados posibles
- **proposed** — en evaluación
- **accepted** — decisión tomada y en vigor
- **deprecated** — ya no aplica
- **superseded** — reemplazada por otro ADR (indicar cuál)

## Convención de nombres
`ADR-[NNN]-[slug-descriptivo].md` — NNN correlativo, slug en kebab-case.
Todo ADR es inmutable al aceptarse. Si cambian las condiciones, crear uno nuevo.
