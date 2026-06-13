---
name: G-INF-005-migrations-runtime
description: db.Database.Migrate() en runtime está prohibido en non-Development — usar step de deploy dedicado
metadata:
  type: feedback
---

No llamar `db.Database.Migrate()` desde el arranque de la aplicación en entornos no-Development.

**Por qué:** en perfil Tipo 2 (multi-instancia), dos réplicas arrancando en paralelo compiten por aplicar la misma migración — riesgo de deadlock, corrupción de esquema o arranque fallido.

**Patrón correcto:**
- En `Program.cs`: verificar `db.Database.GetPendingMigrations()` y lanzar `InvalidOperationException` si hay pendientes en non-Development.
- En Development: `db.Database.Migrate()` es aceptable para agilizar el ciclo local.
- En CI/CD: step explícito `dotnet ef database update` antes del deploy de imagen.

**How to apply:** cualquier PR que toque `Program.cs` o el pipeline de deploy debe verificar que no se reintroduzca `Migrate()` fuera del bloque `IsDevelopment`.

Relacionado: [[G-INF-004-check-constraints-efcore9]]
