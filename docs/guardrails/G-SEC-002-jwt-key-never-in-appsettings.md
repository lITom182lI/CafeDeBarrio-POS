---
id: G-SEC-002
type: validated-decision
scope: TIPO-2
layer: security
trigger: Jwt:Key, appsettings.json, JWT, secreto, clave, knownPlaceholders
linked-ledger: F-02
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se cambia el proveedor de configuración (Vault, Azure Key Vault)"
---

**Regla:** `Jwt:Key` nunca tiene valor real en `appsettings.json`. El valor debe ser el placeholder `"OVERRIDE_VIA_ENV_VAR"` y la clave real se inyecta exclusivamente via variable de entorno o `appsettings.Development.json` (gitignoreado).

**Why:** En F-02 se encontró `"A_VERY_SECURE_KEY_FOR_EF_CORE_MIGRATIONS_1234567890_LONG_ENOUGH_HA"` versionado en el repo. La clave tenía >32 caracteres y evadía la lista `knownPlaceholders` de `Program.cs`, permitiendo que el sistema arrancara en producción firmando tokens con una clave conocida públicamente.

**How to apply:** Al modificar `appsettings.json` o `Program.cs`:
1. `appsettings.json` → `"Key": "OVERRIDE_VIA_ENV_VAR"` (capturado por `knownPlaceholders`).
2. `Program.cs` bloque `!IsDevelopment()` → incluir `RequireConfig(builder.Configuration, "Jwt:Key")`.
3. `dev-setup.ps1` genera la clave real con `RandomNumberGenerator` y la escribe en `appsettings.Development.json`.

```json
// ✅ appsettings.json
"Jwt": { "Key": "OVERRIDE_VIA_ENV_VAR" }

// ❌ Nunca
"Jwt": { "Key": "AlgunValorRealAunqueSeParezcaAUnPlaceholder" }
```
