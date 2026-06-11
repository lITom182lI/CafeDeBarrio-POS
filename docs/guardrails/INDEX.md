# Guardrails — Índice por capa
**Carga:** selectiva — leer solo la sección de la capa que se va a modificar en la tarea actual.

## Infrastructure
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-INF-001 | No usar `IQueryable.GroupBy()` sobre `.Date` con filtro de navegación nula — EF Core 9 no lo traduce | anti-pattern |
| G-INF-002 | `AddHttpClient()` siempre incondicional en `Program.cs` antes de cualquier registro condicional de clientes HTTP | anti-pattern |
| G-INF-003 | No usar `HasData()` para datos de catálogo/negocio — usar `CatalogDataSeeder` idempotente | validated-decision |

## Security
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-SEC-001 | CORS nunca con `AllowAnyHeader()` + `AllowAnyMethod()` — siempre headers y métodos explícitos | validated-decision |

## DevOps / Workflow
| ID | Regla en una línea | Tipo |
|---|---|---|
| G-DEV-001 | Al instruir reemplazo de archivo en Antigravity: especificar "sobreescribir (no append)" explícitamente | anti-pattern |
| G-DEV-002 | En CI/Docker: `github.repository_owner` puede ser uppercase — siempre aplicar `tr '[:upper:]' '[:lower:]'` | anti-pattern |
| G-DEV-003 | En Dockerfile: `dotnet restore` apunta al `.csproj` del API, no a la `.sln` — la solución referencia proyectos fuera del build context | anti-pattern |
| G-DEV-004 | `appsettings.Development.json` gitignoreado — siempre mantener `.template.json` versionado + `dev-setup.ps1` que lo copia | validated-decision |

## Clasificación por scope
- **MUIS-CORE** (aplica a todos los proyectos MUIS): G-SEC-001, G-DEV-001, G-DEV-002
- **TIPO-2** (SaaS Escalable): G-INF-003, G-DEV-003, G-DEV-004
- **PROYECTO** (CafeDeBarrio-POS específico): G-INF-001, G-INF-002
