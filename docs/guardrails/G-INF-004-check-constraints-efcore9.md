---
id: G-INF-004
type: anti-pattern
scope: PROYECTO
layer: infrastructure
trigger: CHECK, constraints, ToTable, EF Core 9, HasCheckConstraint, CS0618
linked-ledger: V3-04
linked-adr: ""
last-reviewed: 2026-06-13
review-when: Upgrade a EF Core 10
---

**Contexto:** `HasCheckConstraint()` directo sobre `EntityTypeBuilder<T>` está obsoleto (CS0618) en EF Core 9.

**Regla:** Toda constraint CHECK debe declararse dentro de `ToTable()`:

```csharp
// ✅ Correcto
builder.ToTable("Transaccion", t => {
    t.HasCheckConstraint("CK_Transaccion_Total_Positivo", "[total] >= 0");
});

// ❌ Obsoleto — CS0618
builder.HasCheckConstraint("CK_Transaccion_Total_Positivo", "[total] >= 0");
```

**Por qué:** EF Core 9 mueve los metadatos de tabla (incluyendo CHECK constraints) al contexto de ToTable(). La forma directa sigue funcionando hoy pero será eliminada en EF Core 10.

**Aplica a:** Todos los `IEntityTypeConfiguration<T>` del proyecto.
