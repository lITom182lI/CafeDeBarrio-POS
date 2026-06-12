---
id: G-FIN-001
type: validated-decision
scope: TIPO-2
layer: backend
trigger: Math.Round, redondeo, IGV, impuesto, MoneyRounding, SUNAT, Nubefact, fiscal
linked-ledger: F-05
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si SUNAT cambia su estándar de redondeo o se integra un nuevo proveedor OSE"
---

**Regla:** Todo redondeo de importes monetarios usa `MoneyRounding.Round()` (`MidpointRounding.AwayFromZero`). Nunca `Math.Round(x, 2)` directamente en lógica fiscal o de negocio.

**Why:** En F-05 `CreateTransaccionHandler` y `SunatOseClient` usaban `Math.Round(x, 2)` sin especificar `MidpointRounding`. El default de .NET es `ToEven` (banker's rounding): `1.235` → `1.23`. SUNAT/Nubefact esperan `AwayFromZero`: `1.235` → `1.24`. La diferencia de S/ 0.01 en valores con `.xx5` genera rechazo de comprobantes fiscales.

**How to apply:**
```csharp
// ✅ Correcto — en cualquier cálculo de impuesto, subtotal o total
using CafeBarrio.Application.Common.Helpers;
var impuesto = MoneyRounding.Round(subtotal * tasaIgv);

// ❌ Nunca en lógica fiscal
var impuesto = Math.Round(subtotal * tasaIgv, 2);
```
Ejecutar `grep "Math\.Round" src/ --include="*.cs"` antes de cada PR que toque cálculos monetarios — debe retornar 0 hits en código de producción.
