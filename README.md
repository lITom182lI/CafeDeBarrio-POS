# CafeDeBarrio POS

Sistema de punto de venta para cafeterías. Tipología MUIS Tipo 1 (Monolítico Simple).

## Componentes

| Componente | Tecnología | Puerto local | Descripción |
|---|---|---|---|
| API | .NET 9 | 5000/5001 | Backend REST — Clean Architecture |
| Dashboard | React 19 + Vite | 5173 | Panel de administración web |
| POS PWA | React 19 + Vite | 5174 | Punto de venta — funciona offline |
| pos-client | WinForms .NET 9 | — | Cliente de escritorio Windows |

## Prerrequisitos

- .NET SDK 9.x — https://dotnet.microsoft.com/download/dotnet/9.0
- Node.js 20+ — https://nodejs.org
- Docker Desktop — https://www.docker.com/products/docker-desktop (para SQL Server local)
- PowerShell 7+ (incluido en Windows 11)

## Quick Start (5 pasos)

**1. Clonar y configurar entorno:**
```bash
git clone <repo-url>
cd CafeDeBarrio-POS
cp .env.example .env
```
Editar `.env`: reemplazar `MSSQL_SA_PASSWORD` con una contraseña segura

**2. Iniciar SQL Server:**
```bash
docker-compose up -d
```

**3. Aplicar migraciones y arrancar la API:**
```bash
dotnet ef database update --project src/CafeBarrio.Infrastructure --startup-project src/CafeBarrio.API
dotnet run --project src/CafeBarrio.API
```
La API queda en `https://localhost:5001`. Usuario admin: `admin@cafedebarrio.com` / `Admin2026!`

**4. Arrancar el Dashboard:**
```bash
cd dashboard
npm install
npm run dev
```
Abre `http://localhost:5173`

**5. Arrancar el POS PWA:**
```bash
cd pos-pwa
npm install
npm run dev
```
Abre `http://localhost:5174`

## Configuración de producción

Copiar `.env.example` como guía. Las siguientes variables de entorno son obligatorias:

| Variable de entorno | Descripción |
|---|---|
| `Jwt__Key` | Secret JWT ≥ 48 chars base64. Generar con `scripts/generate-jwt-secret.ps1` |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión SQL Server de producción |
| `Cors__AllowedOrigin` | URL del Dashboard en producción (ej. `https://mi-dominio.com`) |
| `MSSQL_SA_PASSWORD` | Contraseña SA de SQL Server (solo para docker-compose local) |

La API lee las variables de entorno automáticamente. No es necesario modificar `appsettings.json`.

## Tests

Unit tests backend
```bash
dotnet test tests/CafeBarrio.Tests.Unit
```

Integration tests backend (requiere SQL Server corriendo)
```bash
dotnet test tests/CafeBarrio.Tests.Integration
```

Unit tests POS PWA
```bash
cd pos-pwa && npx vitest run
```

## Estructura del proyecto

```text
src/
CafeBarrio.Domain/          Entidades y contratos (sin dependencias externas)
CafeBarrio.Application/     Casos de uso — Commands, Queries, Validators
CafeBarrio.Infrastructure/  EF Core, repositorios, JWT, Argon2, audit
CafeBarrio.API/             Controllers, Program.cs, middleware

tests/
CafeBarrio.Tests.Unit/      xUnit + NSubstitute — sin base de datos
CafeBarrio.Tests.Integration/ xUnit + SQL Server real — Estrategia 2B (rollback por test)

dashboard/                    React + Recharts — reportes, CRUD, turnos
pos-pwa/                      React PWA offline-first con IndexedDB + sync
pos-client/                   WinForms — cliente de escritorio alternativo
docs/adr/                     Decisiones arquitectónicas (ADR-001, ADR-002)
```

## Decisiones de arquitectura

- [ADR-001](docs/adr/ADR-001-tipologia-muis.md) — Clasificación Tipo 1 MUIS
- [ADR-002](docs/adr/ADR-002-offline-first-pos.md) — Arquitectura offline-first POS
- [Domain Charter](docs/domain_charter_v1.0.0-SEALED-2026-06-05.md) — Modelo de dominio sellado
