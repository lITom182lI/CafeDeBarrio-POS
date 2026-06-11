# ADR-005: CatalogDataSeeder sobre HasData() para datos de catálogo de negocio

**Estado:** ACEPTADO
**Fecha:** 2026-06-11
**Guardrail relacionado:** G-INF-003
**Ledger relacionado:** PROD-05

---

## Contexto

El sistema necesita datos iniciales de catálogo al primer arranque: 3 categorías, 16 productos, Sede 1 y ConfiguracionNegocio. Sin estos datos, el POS es inutilizable desde el primer deploy. El proyecto ya tenía antecedente de remover `HasData()` (`RemoveConfigSeed` migration) por problemas de sostenibilidad.

## Decisión

Implementar `CatalogDataSeeder` — clase idempotente registrada en DI e invocada al startup que verifica existencia antes de insertar. No usar `HasData()` de EF Core para ningún dato de catálogo o negocio.

## Alternativas consideradas

| Alternativa | Razón de descarte |
|---|---|
| `HasData()` en configuraciones EF Core | Bake los datos en migraciones. Cualquier cambio de precio, nombre o nueva categoría requiere migración nueva. Insostenible para catálogo vivo. Ya fue removido del proyecto anteriormente. |
| Script SQL manual | No versionado en código, no reproducible automáticamente, requiere intervención manual en cada despliegue. |
| Datos cargados via API en primer uso | Requiere UI de administración completa antes del primer uso operativo. |

## Consecuencias

### Positivas
- Reproducible: cualquier deploy nuevo arranca con el catálogo completo automáticamente
- Idempotente: correr el seeder múltiples veces no duplica datos
- Cambios de catálogo no requieren migraciones
- Aplica igual en Azure, Docker local y entorno de desarrollo

### Negativas / Trade-offs aceptados
- El seeder corre en cada arranque del API con una verificación `AnyAsync()` — overhead mínimo

## Trigger de revisión

Si se introduce un sistema de gestión de catálogo via UI que reemplace la necesidad del seeder inicial.

## Inmutabilidad

Inmutable a partir de su aceptación. Si el mecanismo de seeding cambia, crear ADR que reemplace a este.
