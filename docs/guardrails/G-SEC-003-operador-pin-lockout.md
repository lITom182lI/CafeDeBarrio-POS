---
id: G-SEC-003
type: validated-decision
scope: TIPO-2
layer: security
trigger: ValidarPin, PIN, lockout, fuerza bruta, FailedPinAttempts, IsLockedOut
linked-ledger: F-03
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se cambia la política de intentos o duración del bloqueo"
---

**Regla:** El endpoint de validación de PIN implementa lockout por identidad de Operador (no solo por IP). Tras 5 intentos fallidos consecutivos, el Operador queda bloqueado 10 minutos. El contador se resetea en éxito.

**Why:** En F-03 el rate limiter usaba `RemoteIpAddress` como clave de partición. Rotar IP evade el límite. La entidad `Operador` no tenía campos de lockout, dejando 10⁶ combinaciones de PIN de 6 dígitos sin defensa por identidad. Se añadieron `FailedPinAttempts`, `IsLockedOut`, `LockedUntilUtc` a la entidad y la lógica en `ValidarPinHandler`.

**How to apply:** Al modificar `ValidarPinHandler` o la entidad `Operador`:
- Verificar lockout activo antes de comprobar el PIN.
- Incrementar `FailedPinAttempts` en cada fallo; bloquear al llegar a 5.
- Resetear `IsLockedOut = false` y `FailedPinAttempts = 0` en PIN correcto.
- El lockout es complementario al rate limiter por IP — ambos deben coexistir.
