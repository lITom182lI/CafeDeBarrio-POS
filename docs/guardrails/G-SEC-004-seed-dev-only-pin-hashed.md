---
id: G-SEC-004
type: validated-decision
scope: TIPO-2
layer: security
trigger: CatalogDataSeeder, seed, PinHash, IsDevelopment, producción, test data
linked-ledger: F-06
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se añade un nuevo seeder con datos de prueba"
---

**Regla:** Los datos de simulación (operadores de prueba, turnos ficticios, transacciones demo) solo se siembran cuando `IHostEnvironment.IsDevelopment()` es `true`. El PIN de cualquier operador sembrado siempre se hashea via `IPasswordHasher`, nunca se almacena en texto plano.

**Why:** En F-06 `CatalogDataSeeder` asignaba `PinHash = "123456"` (texto plano) y ejecutaba el bloque de test data en cualquier entorno, incluyendo producción. Resultado: BD de producción con 10 operadores con PIN conocido y sin hash Argon2.

**How to apply:** En cualquier seeder que cree operadores o datos de prueba:
```csharp
// ✅ Correcto
if (_env.IsDevelopment())
{
    ops.Add(new Operador { PinHash = _hasher.Hash("123456"), ... });
}

// ❌ Nunca
ops.Add(new Operador { PinHash = "123456", ... });        // texto plano
// Y nunca fuera del guard IsDevelopment para test data
```
