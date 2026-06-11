---
id: G-SEC-001
type: validated-decision
scope: MUIS-CORE
layer: security
trigger: CORS, AllowAny, AllowAnyHeader, AllowAnyMethod, WithHeaders, WithMethods, política
linked-ledger: WARN-01
linked-adr: ""
last-reviewed: 2026-06-11
review-when: Si se agregan nuevos headers o métodos HTTP al contrato de la API
---

**Regla:** La política CORS nunca debe usar `AllowAnyHeader()` ni `AllowAnyMethod()` — siempre declarar headers y métodos HTTP explícitos.

**Why:** `AllowAnyHeader()` + `AllowAnyMethod()` expone todos los endpoints a cualquier verbo HTTP sin restricción desde cualquier origen en la whitelist. WARN-01 del proyecto lo identificó como hallazgo de hardening. OWASP y CIS Benchmark exigen políticas CORS restrictivas en sistemas de negocio.

**How to apply:**

```csharp
// CORRECTO: headers y métodos explícitos
options.AddPolicy("CafePosPolicy", policy => policy
    .WithOrigins(allowedOrigins)
    .WithHeaders("Content-Type", "Authorization", "X-Operator-Id")
    .WithMethods("GET", "POST", "PUT", "DELETE"));

// INCORRECTO: política abierta
options.AddPolicy("CafePosPolicy", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()    // nunca
    .AllowAnyMethod());  // nunca
```

Al agregar un nuevo header al contrato de la API, actualizar `WithHeaders` explícitamente. Al agregar un endpoint con verbo no listado, actualizar `WithMethods`.
