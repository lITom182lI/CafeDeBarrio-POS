# CafeDeBarrio POS

Sistema de punto de venta para cafeterías. Tipología MUIS Tipo 2 (SaaS Escalable).

## Componentes

| Componente | Tecnología      | Puerto local | Descripción                        |
|------------|-----------------|-------------|-------------------------------------|
| API        | .NET 9          | 8080        | Backend REST — Clean Architecture  |
| Dashboard  | React 19 + Vite | 5173        | Panel de administración web        |
| POS PWA    | React 19 + Vite | 5174        | Punto de venta — funciona offline  |

## Prerrequisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop) — incluye Docker Compose
- PowerShell 5.1+ (incluido en Windows 10/11)
- Git

> Para desarrollo local (sin Docker) se necesita adicionalmente .NET SDK 9 y Node.js 22+.

## Setup en cualquier PC — 5 comandos

```powershell
git clone https://github.com/lITom182lI/CafeDeBarrio-POS
cd CafeDeBarrio-POS
.\setup.ps1                   # genera .env + .env.local de frontends
docker compose pull
docker compose up -d          # levanta API + SQL Server, migra y seedea automáticamente
```

Luego abrir los frontends en terminales separadas:

```powershell
# Terminal 1 — Dashboard
cd dashboard && npm install && npm run dev

# Terminal 2 — POS PWA
cd pos-pwa && npm install && npm run dev
```

| Servicio  | URL                   | Credenciales                             |
|-----------|-----------------------|------------------------------------------|
| Dashboard | http://localhost:5173 | admin@cafedebarrio.com / (ver setup.ps1) |
| POS PWA   | http://localhost:5174 | PIN del operador configurado en Dashboard|
| API       | http://localhost:8080 | —                                        |

> `setup.ps1` crea automáticamente `dashboard/.env.local` y `pos-pwa/.env.local`
> con el proxy apuntando al API Docker (`:8080`). Sin este paso los frontends
> no conectan con el backend.

## Variables de entorno (.env)

Generadas automáticamente por `setup.ps1`. Referencia completa en `.env.example`.

| Variable         | Descripción                                      |
|------------------|--------------------------------------------------|
| `SA_PASSWORD`    | Contraseña SA de SQL Server en Docker            |
| `JWT_KEY`        | Secreto JWT — mínimo 32 caracteres               |
| `CORS_ORIGIN`    | Orígenes CORS permitidos (separados por coma)    |
| `ADMIN_PASSWORD` | Contraseña inicial del administrador             |
| `SUNAT_ENABLED`  | Activar integración SUNAT (`true`/`false`)       |

## Desarrollo local (sin Docker)

```powershell
# Terminal 1 — API
dotnet run --project src/CafeBarrio.API

# Terminal 2 — Dashboard
cd dashboard && npm install && npm run dev

# Terminal 3 — POS PWA
cd pos-pwa && npm install && npm run dev
```

Requiere SQL Server accesible y `.env` con `ConnectionStrings__DefaultConnection` configurado.

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
