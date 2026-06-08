# Matriz de Trazabilidad de Requisitos — CafeDeBarrio-POS v1.0.0

**Norma de referencia:** MUIS Tipo 1 (Monolítico Simple) — Pilar 9 Ingeniería de Requisitos  
**Fecha:** 2026-06-07  
**Estado:** PRODUCCIÓN

## Instrucciones de uso

La columna **Verificación sin código** contiene un comando PowerShell/bash o
una acción concreta que cualquier persona puede ejecutar para confirmar el
requisito, sin necesidad de leer el código fuente.

Leyenda de comandos:
- `grep` / `Select-String` — busca texto en archivos
- `ls` / `dir` — lista archivos
- `dotnet test` — ejecuta los tests y reporta el resultado
- `curl` / navegador — llama a la API en vivo

---

## Tabla de trazabilidad

| ID | Módulo MUIS | Requisito | Evidencia en repositorio | Test que lo verifica | Verificación sin código |
|----|-------------|-----------|--------------------------|----------------------|------------------------|
| RTM-01 | MUIS_BACKEND P1/P2 | Clean Architecture — 4 capas con dependencias unidireccionales | `src/CafeBarrio.Domain/` `src/CafeBarrio.Application/` `src/CafeBarrio.Infrastructure/` `src/CafeBarrio.API/` | CI: `dotnet build -c Release` (falla si hay dependencias circulares) | `ls src/` → 4 directorios de capa. `dotnet build src/CafeBarrio.sln -c Release` → 0 errores, 0 warnings |
| RTM-02 | MUIS_BACKEND P3 | Tipología Tipo 1 documentada en ADR | `docs/adr/ADR-001-tipologia-muis.md` | — | Abrir `docs/adr/ADR-001-tipologia-muis.md` → Estado: ACEPTADO, Tipo 1 justificado |
| RTM-03 | MUIS_BACKEND P9 | Domain Charter sellado (modelo de dominio inmutable) | `docs/domain_charter_v1.0.0-SEALED-2026-06-05.md` | — | `ls docs/` → archivo con "SEALED-2026-06-05" en el nombre. Abrirlo → sección "Inmutabilidad" presente |
| RTM-04 | MUIS_BACKEND P9 | ADR-002: arquitectura offline documentada | `docs/adr/ADR-002-offline-first-pos.md` | — | Abrir `docs/adr/ADR-002-offline-first-pos.md` → Estado: ACEPTADO, IndexedDB referenciado |
| RTM-05 | MUIS_SECURITY_AUTH | Hashing de contraseñas con Argon2id | `src/CafeBarrio.Infrastructure/Security/Argon2PasswordHasher.cs` | `tests/CafeBarrio.Tests.Unit/Auth/Argon2PasswordHasherTests.cs` (4 tests) | `Select-String -Path "src/CafeBarrio.Infrastructure/Security/Argon2PasswordHasher.cs" -Pattern "Argon2"` → muestra 3+ coincidencias. Alternativa funcional: SMOKE_TEST paso 3.2 (login con credenciales correctas funciona) |
| RTM-06 | MUIS_SECURITY_AUTH | Rate limiting: 5 intentos login por IP cada 5 minutos | `src/CafeBarrio.API/Program.cs` líneas 42–52 | `tests/CafeBarrio.Tests.Integration/Features/Auth/AuthIntegrationTests.cs` | SMOKE_TEST paso 5.1: enviar 6 intentos de login con password errónea → la 6ta respuesta devuelve `429 Too Many Requests` con header `Retry-After` |
| RTM-07 | MUIS_SECURITY_AUTH | JWT con expiración + RBAC (endpoints protegidos) | `src/CafeBarrio.Infrastructure/Security/JwtService.cs` `src/CafeBarrio.API/Program.cs` líneas 23–37 | `tests/CafeBarrio.Tests.Unit/Auth/JwtServiceTests.cs` (3 tests) | SMOKE_TEST paso 5.2: `GET /api/productos` sin header `Authorization` → `401 Unauthorized`. SMOKE_TEST paso 5.3: `GET /api/categorias` sin token → `200 OK` (endpoint público) |
| RTM-08 | MUIS_AUDIT | AuditInterceptor SCOPED — registra CreatedBy y UpdatedBy automáticamente | `src/CafeBarrio.Infrastructure/Persistence/Interceptors/AuditInterceptor.cs` `src/CafeBarrio.Infrastructure/DependencyInjection.cs` línea 20 | `tests/CafeBarrio.Tests.Unit/Auth/ChangePasswordHandlerTests.cs` (SaveChanges verificado) | 1) Login en Dashboard. 2) Crear un producto. 3) Conectar a BD con SSMS o Azure Data Studio: `SELECT CreatedBy, CreatedAt FROM Productos` → campos contienen el email del usuario y timestamp, no son NULL |
| RTM-09 | MUIS_AUDIT | IAuditable implementado en 6 entidades de dominio | `src/CafeBarrio.Domain/Entities/Transaccion.cs:5` `Producto.cs:5` `Turno.cs:5` `Operador.cs:5` `MovimientoCaja.cs:5` `Anulacion.cs:5` | `tests/CafeBarrio.Tests.Unit/Transacciones/CreateTransaccionHandlerTests.cs` | `Select-String -Path "src/CafeBarrio.Domain/Entities/*.cs" -Pattern "IAuditable"` → exactamente 6 archivos con `: IAuditable` |
| RTM-10 | MUIS_DATA_STORAGE | Sin Redis — sin dependencia externa de caché | Ausencia en todos los archivos `.csproj`. Caché de catálogo en cliente: `pos-pwa/src/offline/catalogStore.ts` | CI: `dotnet build` (falla si hubiera paquete Redis no resuelto) | `Select-String -Path "src/**/*.csproj" -Pattern "Redis" -Recurse` → **sin salida** (0 coincidencias). `ls pos-pwa/src/offline/catalogStore.ts` → archivo de caché del lado del cliente |
| RTM-11 | MUIS_OBSERVABILIDAD | Health endpoint `/health` con verificación de BD | `src/CafeBarrio.API/Program.cs` líneas 79–80 y 184 | `tests/CafeBarrio.Tests.Integration/Base/IntegrationTestBase.cs` (verifica conexión en cada test) | Con la API corriendo: abrir en navegador `https://localhost:5001/health` → respuesta `{"status":"Healthy"}` en menos de 1 segundo |
| RTM-12 | MUIS_QA | Unit tests — cobertura ≥80% en lógica de dominio | `tests/CafeBarrio.Tests.Unit/` — 13 archivos cubriendo Auth, Anulaciones, MovimientosCaja, Transacciones, Turnos, Productos, Operadores, Reportes | Todos los archivos en `tests/CafeBarrio.Tests.Unit/` | `dotnet test tests/CafeBarrio.Tests.Unit` → ≥33 tests passed, 0 failed, 0 skipped |
| RTM-13 | MUIS_QA | Integration tests — Estrategia 2B (SqlTransaction rollback por test, BD real) | `tests/CafeBarrio.Tests.Integration/Base/IntegrationTestBase.cs` (patrón BeginTransaction + Rollback en Dispose) — 4 suites: Auth, Transacciones, Operadores, Turnos | Todos los archivos en `tests/CafeBarrio.Tests.Integration/Features/` | `dotnet test tests/CafeBarrio.Tests.Integration` (requiere Docker con SQL Server activo) → ≥8 tests passed, 0 failed |
| RTM-14 | MUIS_DEVOPS | CI/CD automatizado: build Release + unit tests + integration tests + vitest | `.github/workflows/ci.yml` — jobs: `build-and-test` (ubuntu/windows) + `integration-tests` (ubuntu + SQL Server container) | Todos los tests anteriores ejecutados automáticamente en CI | GitHub → pestaña **Actions** → último commit en `main` → 2 jobs con ✅ verde. Sin acceso a GitHub: `cat .github/workflows/ci.yml` → ver ambos jobs definidos |
| RTM-15 | MUIS_DEVOPS | Secrets fuera del repositorio — configuración por variables de entorno | `.env.example` líneas 1–14 (solo placeholders). `.gitignore` líneas 490, 494–495 (archivos de config excluidos) | — | Abrir `.env.example` → cada valor dice `REEMPLAZAR_...` o `YOUR_...`. `git status` → `.env` no aparece como tracked. `Select-String -Path ".gitignore" -Pattern "appsettings.Production"` → 2 coincidencias |
| RTM-16 | MUIS_FRONTEND_WEB | WCAG AA — modales con atributos de accesibilidad obligatorios | `dashboard/src/components/ProductoModal.tsx:66–68` `dashboard/src/components/OperadorModal.tsx:53–55` | — | En Dashboard: abrir modal "Nuevo Producto" → clic derecho en el fondo oscuro → **Inspeccionar** → buscar el `div` del modal → debe contener `role="dialog"`, `aria-modal="true"`, `aria-labelledby` |
| RTM-17 | MUIS_FRONTEND_MOBILE | Offline-first — IndexedDB + cola de transacciones + sincronización | `pos-pwa/src/offline/db.ts` `pos-pwa/src/offline/pendingStore.ts` `pos-pwa/src/offline/syncService.ts` `pos-pwa/src/offline/catalogStore.ts` | `pos-pwa/src/test/syncService.test.ts` (4 tests) `pos-pwa/src/test/adapter.test.ts` (10 tests) | SMOKE_TEST pasos 4.7–4.8: detener la API → hacer una venta en POS → venta queda como "pendiente" → reiniciar API → en ≤30 s la venta aparece sincronizada en Dashboard. Alternativa: DevTools → **Application** → **IndexedDB** → base de datos `cafe-barrio-pos` visible |
| RTM-18 | MUIS_DEVOPS | Scripts de operación presentes y ejecutables | `scripts/build-release.ps1` `scripts/generate-jwt-secret.ps1` | — | `ls scripts/` → 2 archivos. Ejecutar `scripts/generate-jwt-secret.ps1` → imprime un string base64 de ≥60 caracteres distinto en cada ejecución |

---

## Cobertura por módulo MUIS

| Módulo | Requisitos trazados | Estado |
|--------|--------------------|----|
| MUIS_BACKEND (P1/P2/P3/P9) | RTM-01, RTM-02, RTM-03, RTM-04 | ✅ |
| MUIS_SECURITY_AUTH | RTM-05, RTM-06, RTM-07 | ✅ |
| MUIS_AUDIT | RTM-08, RTM-09 | ✅ |
| MUIS_DATA_STORAGE | RTM-10 | ✅ |
| MUIS_OBSERVABILIDAD | RTM-11 | ✅ |
| MUIS_QA_TESTING | RTM-12, RTM-13 | ✅ |
| MUIS_DEVOPS | RTM-14, RTM-15, RTM-18 | ✅ |
| MUIS_FRONTEND_WEB | RTM-16 | ✅ |
| MUIS_FRONTEND_MOBILE | RTM-17 | ✅ |

---

## Procedimiento de re-verificación

Ante cada nueva versión, ejecutar en orden:

```powershell
# 1. Build
dotnet build src/CafeBarrio.sln -c Release

# 2. Unit tests
dotnet test tests/CafeBarrio.Tests.Unit

# 3. Integration tests (requiere Docker SQL Server)
dotnet test tests/CafeBarrio.Tests.Integration

# 4. Frontend tests
cd pos-pwa; npx vitest run; cd ..

# 5. Health check (con API corriendo)
Invoke-WebRequest https://localhost:5001/health | Select-Object -ExpandProperty Content

# 6. Sin Redis
Select-String -Path "src/**/*.csproj" -Pattern "Redis" -Recurse
```

Resultado esperado: todos los tests passed, health = Healthy, Redis = 0 coincidencias.
