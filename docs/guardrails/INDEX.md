# Guardrails — Índice por capa
**Carga:** selectiva — leer solo la sección de la capa que se va a modificar en la tarea actual.

## Infrastructure
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-INF-001 | No usar `IQueryable.GroupBy()` sobre `.Date` con filtro de navegación nula — EF Core 9 no lo traduce | anti-pattern |
| G-INF-002 | `AddHttpClient()` siempre incondicional en `Program.cs` antes de cualquier registro condicional de clientes HTTP | anti-pattern |
| G-INF-003 | No usar `HasData()` para datos de catálogo/negocio — usar `CatalogDataSeeder` idempotente | validated-decision |
| G-INF-004 | Toda constraint CHECK debe declararse dentro de `ToTable()` en EF Core 9 (obsoleto CS0618) | anti-pattern |
| G-INF-005 | `db.Database.Migrate()` prohibido en runtime non-Development; usar step de deploy dedicado | anti-pattern |
| G-DB-002 | `UseSqlServer` requiere `EnableRetryOnFailure` (5, 10s) + `CommandTimeout(30)` | validated-decision |
| G-DB-003 | `MovimientoCaja` y registros de auditoría financiera: `DeleteBehavior.Restrict`, nunca `Cascade` | validated-decision |
| G-DB-004 | `CreatedAt` lo gestiona `AuditInterceptor` exclusivamente — nunca `HasDefaultValueSql` en entidades `IAuditable` | validated-decision |

## Security
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-SEC-001 | CORS nunca con `AllowAnyHeader()` + `AllowAnyMethod()` — siempre headers y métodos explícitos | validated-decision |
| G-SEC-002 | `Jwt:Key` nunca con valor real en `appsettings.json` — siempre `"OVERRIDE_VIA_ENV_VAR"` + `RequireConfig` en prod | validated-decision |
| G-SEC-003 | `ValidarPin` implementa lockout por identidad de Operador: 5 fallos → bloqueado 10 min | validated-decision |
| G-SEC-004 | Seeds de test data solo en `IsDevelopment()`; `PinHash` siempre via `IPasswordHasher` | validated-decision |
| G-SEC-005 | Token de Operador incluye `security_stamp`; rotarlo en cada cambio de PIN | validated-decision |
| G-SEC-006 | `SedeId` en handlers Operador-facing siempre validado contra claim `sede_id` del JWT | validated-decision |

## DevOps / Workflow
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-DEV-001 | Al instruir reemplazo de archivo en Antigravity: especificar "sobreescribir (no append)" explícitamente | anti-pattern |
| G-DEV-002 | En CI/Docker: `github.repository_owner` puede ser uppercase — siempre aplicar `tr '[:upper:]' '[:lower:]'` | anti-pattern |
| G-DEV-003 | En Dockerfile: `dotnet restore` apunta al `.csproj` del API, no a la `.sln` — la solución referencia proyectos fuera del build context | anti-pattern |
| G-DEV-004 | `appsettings.Development.json` gitignoreado — siempre mantener `.template.json` versionado + `dev-setup.ps1` que lo copia | validated-decision |

## Architecture
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-ARCH-001 | Nunca capturar `DbUpdateException` en Application — Infrastructure traduce a `PersistenceException` via `UnitOfWork` | validated-decision |

## Finance / Fiscal
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-FIN-001 | Todo redondeo monetario usa `MoneyRounding.Round()` (`AwayFromZero`) — nunca `Math.Round(x, 2)` directamente | validated-decision |

## Clasificación por scope
- **MUIS-CORE** (aplica a todos los proyectos MUIS): G-SEC-001, G-DEV-001, G-DEV-002
- **TIPO-2** (SaaS Escalable): G-INF-003, G-DEV-003, G-DEV-004, G-ARCH-001, G-DB-002, G-DB-003, G-DB-004, G-SEC-002, G-SEC-003, G-SEC-004, G-SEC-005, G-SEC-006, G-FIN-001
- **PROYECTO** (CafeDeBarrio-POS específico): G-INF-001, G-INF-002
