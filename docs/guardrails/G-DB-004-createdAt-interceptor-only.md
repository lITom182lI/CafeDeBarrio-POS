---
id: G-DB-004
type: validated-decision
scope: TIPO-2
layer: infrastructure
trigger: CreatedAt, HasDefaultValueSql, GETUTCDATE, AuditInterceptor, IAuditable
linked-ledger: F-13
linked-adr: ""
last-reviewed: 2026-06-12
review-when: "Si se reemplaza AuditInterceptor o se cambia de SQL Server"
---

**Regla:** Las entidades que implementan `IAuditable` nunca usan `HasDefaultValueSql("GETUTCDATE()")` en su configuración EF Core. `CreatedAt` es responsabilidad exclusiva del `AuditInterceptor`.

**Why:** En F-13 `Transaccion` y `Producto` tenían `HasDefaultValueSql("GETUTCDATE()")`. EF Core interpreta esto como `ValueGeneratedOnAdd`, pudiendo omitir el valor del interceptor en el INSERT y usar el DEFAULT de SQL Server. Resultado: `CreatedAt` podía diferir del valor establecido por el interceptor (diferencia de zona horaria o milisegundos) y el comportamiento era impredecible según el estado del change tracker.

**How to apply:** En `DbContext.OnModelCreating` para entidades `IAuditable`:
```csharp
// ✅ Correcto
e.Property(t => t.CreatedAt).HasColumnName("created_at");

// ❌ Nunca en entidades IAuditable
e.Property(t => t.CreatedAt)
 .HasColumnName("created_at")
 .HasDefaultValueSql("GETUTCDATE()");
```
