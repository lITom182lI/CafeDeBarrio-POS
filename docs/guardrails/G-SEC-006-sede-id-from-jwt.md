---
id: G-SEC-006
type: validated-decision
scope: TIPO-2
layer: security
trigger: SedeId, IDOR, ICurrentUserService, sede_id, token, handler
linked-ledger: F-11
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se añade multi-sede o un Operador puede pertenecer a varias sedes"
---

**Regla:** En handlers Operador-facing, `SedeId` se valida contra el claim `sede_id` del JWT. Si el token tiene `sede_id` y no coincide con `request.SedeId`, retornar `Auth.ForbiddenSede`. Los tokens de Admin no llevan `sede_id` — `ICurrentUserService.SedeId` es null → sin restricción.

**Why:** En F-11 los handlers aceptaban `SedeId` del body/query sin validar. Un Operador autenticado de Sede 1 podía enviar `SedeId=2` y acceder a datos de otra sede (IDOR clásico).

**How to apply:** En cualquier handler Operador-facing que acepte `SedeId`:
```csharp
// ✅ Guard estándar — añadir al inicio del Handle tras obtener _currentUser
if (_currentUser.SedeId is not null && _currentUser.SedeId != request.SedeId)
    return Result<...>.Failure(new Error("Auth.ForbiddenSede",
        "No tienes acceso a esta sede."));
```
Handlers afectados actualmente: `CreateTransaccionHandler`, `AbrirTurnoHandler`, `GetTurnoActivoHandler`.
Handlers exentos (Admin-only, `SedeId` null): reportes, anulaciones, configuración.
