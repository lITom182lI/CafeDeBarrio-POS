---
id: G-SEC-005
type: validated-decision
scope: TIPO-2
layer: security
trigger: GenerateOperadorToken, SecurityStamp, token revocado, cambio de PIN, IJwtService
linked-ledger: F-10
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se cambia el mecanismo de autenticación de Operador"
---

**Regla:** El token JWT de Operador siempre incluye el claim `security_stamp`. El stamp se rota en cada cambio de PIN. `OnTokenValidated` en `Program.cs` valida el stamp contra la BD para ambos roles (Admin y Operador).

**Why:** En F-10 `GenerateOperadorToken` no incluía `security_stamp`. `OnTokenValidated` hacía early return cuando `stampClaim is null`, haciendo los tokens de Operador irrevocables — un cambio de PIN no invalidaba sesiones abiertas.

**How to apply:**
- `JwtService.GenerateOperadorToken` → incluir `new Claim("security_stamp", securityStamp)`.
- `UpdateOperadorHandler` (cambio de PIN) → rotar `operador.SecurityStamp = Guid.NewGuid().ToString("N")`.
- `OnTokenValidated` → branch por `roleClaim == "Operador"` que consulta `db.Operadores`.
- `IJwtService.GenerateOperadorToken` recibe `securityStamp` y `sedeId` como parámetros — no obtenerlos del DbContext dentro del servicio.
