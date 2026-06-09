# VALIDATION REPORT — UoW Atomicity Fix
**ID:** MUIS-SYNC-01  
**Severidad:** CRÍTICO  
**Módulo:** Infrastructure → BaseRepository  
**Fecha Auditoría:** 2026-06-09  
**Estado:** ⏳ PENDIENTE

---

## 1. Resumen Ejecutivo

`BaseRepository<T>.AddAsync()` ejecuta `SaveChangesAsync()` internamente. Los 5 handlers que lo invocan también llaman `_uow.SaveChangesAsync(ct)` explícitamente, generando **doble commit no atómico**. En `CreateTransaccionHandler`, el primer commit persiste la Transacción y el segundo persiste la reducción de inventario. Un fallo entre ambos deja el stock inconsistente.

| Diagnóstico | Antes | Después |
|-------------|-------|---------|
| Commits por operación Create | 2 (no atómicos) | 1 (atómico) |
| Riesgo stock incorrecto | REAL | ELIMINADO |
| Contrato IRepository<T>.AddAsync | Save implícito | Stage only |
| PK obtenido desde | BaseRepository (post-save 1) | Entidad (post-save UoW) |

---

## 2. Tabla de Riesgos por Archivo

| Archivo | Tipo Cambio | Riesgo de Regresión | Impacto si no se hace |
|---------|-------------|--------------------|-----------------------|
| `BaseRepository.cs` | Eliminar SaveChangesAsync de AddAsync | Bajo — quita un save, no añade lógica | Stock inconsistente en producción |
| `CreateTransaccionHandler.cs` | Usar `transaccion.TransaccionId` post-UoW | Bajo — misma variable, diferente momento de lectura | ID correcto solo por coincidencia del doble save |
| `CreateAnulacionHandler.cs` | Usar `anulacion.AnulacionId` post-UoW | Bajo | Igual |
| `AbrirTurnoHandler.cs` | Usar `turno.TurnoId` post-UoW | Bajo | Igual |
| `CreateProductoHandler.cs` | Usar `producto.ProductoId` post-UoW | Bajo | Igual |
| `CreateMovimientoCajaHandler.cs` | Usar `movimiento.MovimientoCajaId` post-UoW | Bajo | Igual |
| Tests Unit (`*HandlerTests.cs`) | Cambiar `result.Value.Should().Be(N)` → `IsSuccess` | Bajo — sin BD real el PK siempre es 0 | Tests pasan por razón incorrecta |

**Handlers SIN cambio requerido** (ya usan patrón correcto in-memory):  
`CerrarTurnoHandler`, `UpdateOperadorHandler`, `UpdateProductoHandler`, `DeleteOperadorHandler`, `ChangePasswordHandler`

---

## 3. Diffs Exactos

### `BaseRepository.cs` — `AddAsync`

```diff
-public async Task<Result<int>> AddAsync(T entity, CancellationToken ct = default)
-{
-    try
-    {
-        Context.Set<T>().Add(entity);
-        await Context.SaveChangesAsync(ct);
-        var pkValue = (int)Context.Entry(entity).Properties
-            .First(p => p.Metadata.IsPrimaryKey())
-            .CurrentValue!;
-        return Result<int>.Success(pkValue);
-    }
-    catch (Exception ex)
-    {
-        return Result<int>.Failure(new Error("Db.AddError", ex.Message));
-    }
-}

+public Task<Result<int>> AddAsync(T entity, CancellationToken ct = default)
+{
+    try
+    {
+        Context.Set<T>().Add(entity);
+        return Task.FromResult(Result<int>.Success(0));
+    }
+    catch (Exception ex)
+    {
+        return Task.FromResult(Result<int>.Failure(new Error("Db.AddError", ex.Message)));
+    }
+}
```

### Patrón Handler (aplica a los 5)

```diff
-var result = await _repo.AddAsync(entity, ct);
-// ... operaciones adicionales ...
-await _uow.SaveChangesAsync(ct);
-return Result<int>.Success(result.Value);

+await _repo.AddAsync(entity, ct);
+// ... operaciones adicionales ...
+await _uow.SaveChangesAsync(ct);   // único commit — EF popula entity.PkId aquí
+return Result<int>.Success(entity.PkId);
```

### Unit Tests — Handlers con AddAsync

```diff
-_repo.AddAsync(Arg.Any<TEntity>(), Arg.Any<CancellationToken>())
-     .Returns(Result<int>.Success(77));
-result.Value.Should().Be(77);

+_repo.AddAsync(Arg.Any<TEntity>(), Arg.Any<CancellationToken>())
+     .Returns(Result<int>.Success(0));
+result.IsSuccess.Should().BeTrue();
```

---

## 4. Criterios de Aceptación (Definition of Done)

### Build
- [ ] `dotnet build src/CafeBarrio.sln --configuration Release` → **0 errores, 0 warnings nuevos**

### Unit Tests
- [ ] `dotnet test tests/CafeBarrio.Tests.Unit` → **todos pasan**
- [ ] Ningún test falla por `result.Value` incorrecto

### Integration Tests
- [ ] `dotnet test tests/CafeBarrio.Tests.Integration` → **todos pasan**
- [ ] `CreateTransaccionHandler` integration test retorna PK > 0
- [ ] Stock del producto reducido en el mismo commit que la transacción

### Verificación Manual
- [ ] Crear transacción en POS PWA → aparece en Dashboard → stock reducido correctamente
- [ ] No existen transacciones sin reducción de stock en la BD

---

## 5. Resultados de Ejecución

> *Completar después de ejecutar los cambios*

| Check | Resultado | Notas |
|-------|-----------|-------|
| Build | ✅ | 0 errores, 0 warnings nuevos |
| Unit Tests | ✅ | Todos pasan (40 tests) |
| Integration Tests | ❌ | SQL Server no accesible (Docker no está corriendo) |
| Prueba Manual | ⏳ | Pendiente |

**Ejecutado por:** Antigravity (Claude-sonnet-4-6 via CLI)  
**Fecha ejecución:** 2026-06-09  
**Commit hash:** (En proceso)  
**Estado final:** ⏳ PENDIENTE → (Bloqueado temporalmente por falta de BD para Integration Tests)

---

*Auditoría Arquitectónica MUIS — CafeDeBarrio-POS — Plan de Sincronización Paso 1*
