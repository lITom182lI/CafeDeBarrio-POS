# ADR-006: SQL Server dev en Docker sobre dependencia de SQLEXPRESS local

**Estado:** ACEPTADO
**Fecha:** 2026-06-11
**Guardrail relacionado:** G-DEV-004
**Ledger relacionado:** DEVX-01

---

## Contexto

El equipo tiene 3 personas. El `appsettings.Development.json` original apuntaba a `Server=localhost,1434` con Windows Auth — instancia específica de una sola máquina (`SQLEXPRESS01`). Al clonar el repositorio, los otros desarrolladores no tenían esa instancia configurada y el API no arrancaba. No existía ningún proceso de onboarding documentado.

## Decisión

Proveer `docker-compose.dev.yml` con SQL Server Express en contenedor Docker en puerto 1433, con credenciales de desarrollo conocidas, y `scripts/dev-setup.ps1` que levanta el contenedor, crea `appsettings.Development.json` desde template con JWT key generado automáticamente, instala dependencias y aplica migraciones EF Core. Un solo comando post-clonado levanta el entorno completo.

## Alternativas consideradas

| Alternativa | Razón de descarte |
|---|---|
| Documentar instalación de SQLEXPRESS en README | Proceso largo, error-prone y no reproducible. Cada dev configura diferente. |
| SQLite para desarrollo | Dialectos SQL distintos entre SQLite y SQL Server causan que algunas migraciones EF Core fallen o se comporten diferente en dev vs. producción. |
| Base de datos dev compartida en red local | Introduce dependencia de conectividad de red, conflictos de datos entre sesiones paralelas y riesgo de pérdida de datos del resto del equipo. |

## Consecuencias

### Positivas
- Entorno reproducible: `git clone` + `.\scripts\dev-setup.ps1` = sistema funcionando
- Independiente de configuración local de Windows/SQL Server
- Mismo SQL Server 2022 que producción — sin divergencia de dialectos
- Puerto 1433 libre (SQLEXPRESS01 local usa 1434)

### Negativas / Trade-offs aceptados
- Requiere Docker Desktop instalado — prerequisito documentado y verificado por el setup script
- Contraseña `DevLocal123!` hardcodeada en `docker-compose.dev.yml` — aceptable solo para desarrollo local, nunca para producción

## Trigger de revisión

Si el equipo crece y se decide usar una BD dev compartida, o si se adopta Dev Containers (`.devcontainer`).

## Inmutabilidad

Inmutable a partir de su aceptación. Si el mecanismo de dev cambia, crear ADR que reemplace a este.
