---
id: G-DB-003
type: validated-decision
scope: TIPO-2
layer: infrastructure
trigger: MovimientoCaja, DeleteBehavior, Cascade, Restrict, Turno, auditoría financiera
linked-ledger: F-09
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se añaden nuevas entidades de auditoría financiera con FK a Turno"
---

**Regla:** `MovimientoCaja` y cualquier registro de auditoría financiera usa `DeleteBehavior.Restrict` en su FK a `Turno`. Nunca `Cascade`.

**Why:** En F-09 `MovimientoCajaConfiguration` tenía `OnDelete(DeleteBehavior.Cascade)`. Si un Turno se eliminaba, todos sus movimientos de caja (retiros, ingresos, cierres) se borraban silenciosamente. Son registros financieros — su eliminación debe ser un error explícito, no una operación silenciosa.

**How to apply:** En cualquier `IEntityTypeConfiguration<T>` donde `T` sea un registro de auditoría financiera con FK a `Turno`:
```csharp
// ✅ Correcto
builder.HasOne(m => m.Turno)
       .WithMany(t => t.Movimientos)
       .HasForeignKey(m => m.TurnoId)
       .OnDelete(DeleteBehavior.Restrict);

// ❌ Nunca para registros financieros
.OnDelete(DeleteBehavior.Cascade);
```
Crear migración después de cada cambio de `DeleteBehavior`.
