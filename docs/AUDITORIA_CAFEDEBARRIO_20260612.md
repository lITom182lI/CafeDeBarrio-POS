# Auditoría Técnica MUIS — Sistema CaféDeBarrio POS
**Fecha:** 12 de junio de 2026  
**Clasificación:** Tipo 2 (SaaS Escalable)  
**Auditor:** Senior Backend/DevOps/Security  
**Estado:** APTO CON CAMBIOS OBLIGATORIOS — **NO desplegar a producción sin cerrar P0**

---

## 📋 Resumen Ejecutivo

`cafedebarrio` es un sistema **bien arquitecturado** con base sólida (Clean Architecture, CQRS/MediatR, Argon2id, outbox SUNAT, gobernanza MUIS documentada). Sin embargo, **no está listo para producción** por 6 defectos concretos que producirán fallas de seguridad, integridad de datos y operación:

| Hallazgo | Área | Riesgo |
|----------|------|--------|
| **F-01** Replay idempotente devuelve `TransaccionId=0` | Backend/BD | Ventas reintentadas sin ticket real |
| **F-02** Clave JWT versionada evade guard | Seguridad | Forja de tokens Admin |
| **F-03** Rate limit evadible + Operador sin lockout | Seguridad | Fuerza bruta de PIN |
| **F-04** Sin retry/timeout de BD | BD/Resilencia | Fallas ante cortes transitorios |
| **F-05** Redondeo IGV con `MidpointRounding` implícito | Fiscal | Divergencia de centavos vs SUNAT |
| **F-06** PIN `"123456"` en seed | Seguridad | Operador con PIN conocido |

**Conclusión:** 2 críticos de seguridad + 1 bug funcional + 3 altos impacto. Resolver P0 + demostrar tests de idempotencia/SUNAT/concurrencia → **APTO PARA PRODUCCIÓN**.

---

## 🔴 Hallazgos Críticos (P0 — Bloquean Deploy)

### F-01: Bug confirmado — Idempotencia devuelve `TransaccionId=0`

**Archivo:** `src/CafeBarrio.Application/Features/Transacciones/Commands/CreateTransaccion/CreateTransaccionHandler.cs:117-122`

**Evidencia:**
```csharp
await _idempotencyRecords.AddAsync(new IdempotencyRecord
{
    IdempotencyKey = request.IdempotencyKey,
    TransaccionId = transaccion.TransaccionId,  // ← transaccion.TransaccionId = 0 aquí
    CreatedAtUtc = DateTime.UtcNow
}, ct);

try
{
    await _uow.SaveChangesAsync(ct);  // ← Identity generado DESPUÉS
}
```

**Problema:** El FK se asigna **antes** de que la identidad sea generada por la BD. Verificación de código confirmó que `IdempotencyRecord` no tiene relación de navegación (`HasOne`) en `CafeBarrioDbContext.cs:35-40`, por lo que EF Core **no hace fix-up** automático. Resultado:
- Primera venta: responde bien (Id leído tras SaveChanges)
- Reintento con mismo `IdempotencyKey`: devuelve `Success(0)` (líneas 37-40)

Como `pos-pwa` reintenta tras reconexión, este **es exactamente el escenario que la idempotencia debía proteger**.

**Acción correctiva:**
```csharp
// Opción A: Dos SaveChanges en la misma transacción
await _transacciones.AddAsync(transaccion, ct);
await _uow.SaveChangesAsync(ct);  // Genera Identity

await _idempotencyRecords.AddAsync(new IdempotencyRecord
{
    IdempotencyKey = request.IdempotencyKey,
    TransaccionId = transaccion.TransaccionId,  // ← Ya tiene el Id real
    CreatedAtUtc = DateTime.UtcNow
}, ct);
await _uow.SaveChangesAsync(ct);

// Opción B: Configurar la relación FK en CafeBarrioDbContext
modelBuilder.Entity<IdempotencyRecord>(e =>
{
    e.HasOne<Transaccion>()
     .WithMany()
     .HasForeignKey(r => r.TransaccionId);
});
```

**Verificable:** Nuevo test:
```csharp
[Fact]
public async Task Handle_SameIdempotencyKey_ReturnsSameNonZeroId()
{
    var result1 = await _sut.Handle(BuildCommand(IdempotencyKey: "key-123"), ct);
    var result2 = await _sut.Handle(BuildCommand(IdempotencyKey: "key-123"), ct);
    
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();
    result1.Value.Should().Be(result2.Value);
    result1.Value.Should().NotBe(0);  // ← Key requirement
}
```

---

### F-02: Clave JWT versionada que evade el validador

**Archivo:** `src/CafeBarrio.API/appsettings.json:27`

**Evidencia:**
```json
"Jwt": {
  "Key": "A_VERY_SECURE_KEY_FOR_EF_CORE_MIGRATIONS_1234567890_LONG_ENOUGH_HA",
  "Issuer": "CafeDeBarrio",
  "Audience": "CafeDeBarrioClients",
  "ExpiryHours": "4"
}
```

**Problema:** La clave está en el repositorio público (si es público) y tiene >32 caracteres, por lo que **evade el check de `Program.cs:34-51`** (lista de placeholders reconocidos). Si en producción no se define `Jwt__Key` por variable de entorno, la API arranca firmando tokens con una clave conocida del repo.

```csharp
// Program.cs:68-72 en NO-dev solo exige ConnectionString y CORS, NO la Jwt:Key
if (!builder.Environment.IsDevelopment())
{
    RequireConfig(builder.Configuration, "ConnectionStrings:DefaultConnection");
    RequireConfig(builder.Configuration, "Cors:AllowedOrigin");
    // ← Falta: RequireConfig(builder.Configuration, "Jwt:Key");
}
```

**Acción correctiva:**
1. Eliminar la clave de `appsettings.json` (dejar vacío o placeholder reconocible)
2. Expandir la lista de `knownPlaceholders`:
   ```csharp
   var knownPlaceholders = new[]
   {
       "REEMPLAZAR_CON_SECRETO_MINIMO_32_CARACTERES",
       "OVERRIDE_VIA_ENV_VAR",
       "your-secret-key",
       "changeme",
       "secret",
       "A_VERY_SECURE_KEY"  // ← Añadir
   };
   ```
3. Exigir en prod: `RequireConfig(builder.Configuration, "Jwt:Key");`
4. Purgar del historial git: `git filter-branch --tree-filter 'rm -f src/CafeBarrio.API/appsettings.json' HEAD`
5. Rotar la clave en todas las instancias

**Verificable:** Arranque en prod sin `Jwt__Key` debe lanzar `InvalidOperationException`.

---

### F-03: Rate limiting evadible + Operador sin lockout

**Archivos:**
- `Program.cs:126-127` (proxies vaciados)
- `Program.cs:144, 156, 168` (uso de `RemoteIpAddress`)
- `OperadoresController.cs:93` (validación de PIN sin lockout)

**Problema:**
```csharp
options.KnownProxies.Clear();        // ← Vacía proxies confiables
options.KnownNetworks.Clear();       // ← Vacía redes confiables

var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon";
// Sin proxies configurados, un atacante rota X-Forwarded-For y obtiene cuota nueva
```

Consecuencias:
- **Fuerza bruta de login:** 20 intentos / 15 min por IP → atacante rota IP, 20 × N intentos sin tope
- **Fuerza bruta de PIN:** 10 intentos / 10 min por (IP:OperadorId) → Operador **no tiene lockout**, así que mismo operador puede fallar PIN indefinidamente (10⁶ combinaciones de 6 dígitos)

**Acción correctiva:**
1. Configurar proxies reales en appsettings:
   ```json
   "ReverseProxy": {
     "TrustedProxyIPs": ["10.0.0.1", "192.168.1.254"]  // Nginx, load balancer, etc.
   }
   ```
2. Implementar lockout de Operador (análogo a Usuario):
   - Campos: `FailedPinAttempts`, `IsLockedOut`, `LockedUntilUtc`
   - Migración para agregar campos
   - Lógica en `ValidarPinHandler`: tras 5 intentos fallidos, bloquear 15 min
   - Resetear contador en PIN exitoso

**Verificable:**
```csharp
[Fact]
public async Task ValidarPin_After5Failures_LocksOperador()
{
    var result1 = await _handler.Handle(BuildCommand(pin: "wrong"), ct);
    // × 4 más
    var result5 = await _handler.Handle(BuildCommand(pin: "wrong"), ct);
    
    result5.IsSuccess.Should().BeFalse();
    result5.Errors.Should().Contain(e => e.Code == "Operador.Locked");
}
```

---

### F-04: Sin retry/timeout/pool de conexión de BD

**Archivo:** `src/CafeBarrio.Infrastructure/DependencyInjection.cs:24-26`

**Evidencia:**
```csharp
services.AddDbContext<CafeBarrioDbContext>((sp, options) =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));
// ← Sin EnableRetryOnFailure, sin CommandTimeout, sin pool
```

**Problema:** Cualquier microcorte de red (2–3 segundos) falla la operación sin reintento; reportes con múltiples JOINs pueden exceder el timeout default de 30s.

**Acción correctiva:**
```csharp
services.AddDbContext<CafeBarrioDbContext>((sp, options) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    options.UseSqlServer(
        connectionString,
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 5,
                errorNumbersToAdd: null);
            sqlServerOptions.CommandTimeout(120);  // 2 minutos para reportes
        })
        .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
}, ServiceLifetime.Scoped);
```

Agregar pool a la connection string en `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=CafeDeBarrio;Max Pool Size=20;Min Pool Size=5;..."
}
```

Nota: al activar `EnableRetryOnFailure`, las transacciones explícitas requieren una `IExecutionStrategy` que respete el retry. Actualizar `UnitOfWork.BeginTransactionAsync`:
```csharp
public async Task BeginTransactionAsync(CancellationToken ct = default)
{
    var strategy = _context.Database.CreateExecutionStrategy();
    await strategy.ExecuteAsync(async () =>
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            _transaction = _context.Database.CurrentTransaction;
            _ownsTransaction = false;
            return;
        }
        _transaction = await _context.Database.BeginTransactionAsync(ct);
        _ownsTransaction = true;
    });
}
```

**Verificable:** Test de conexión fallida intermitente con TransientFaultHandling simulator.

---

### F-05: Redondeo de IGV con modo implícito

**Archivos:**
- `CreateTransaccionHandler.cs:89` → `Math.Round(subtotal * tasaIgv, 2)`
- `SunatOseClient.cs` → cálculos de IGV línea

**Problema:** `Math.Round` sin `MidpointRounding` explícito usa el default de .NET (**`ToEven` / banker's rounding**). Ejemplo:
- 0.125 redondea a 0.12 (ToEven)
- 0.135 redondea a 0.14 (ToEven)

SUNAT/Nubefact probablemente espera **AwayFromZero** (0.125 → 0.13). Resultado: **divergencia de centavos** en comprobantes fiscales.

**Acción correctiva:** Crear helper e implementar cálculo alineado con SUNAT:
```csharp
// src/CafeBarrio.Application/Common/MoneyRounding.cs
public static class MoneyRounding
{
    public const MidpointRounding Mode = MidpointRounding.AwayFromZero;
    
    public static decimal Round(decimal value, int decimals = 2)
        => Math.Round(value, decimals, Mode);
    
    public static decimal CalculateIgv(decimal subtotalLinea, decimal tasaIgv)
    {
        // IGV por línea (nunca agregar primero y redondear después)
        return Round(subtotalLinea * tasaIgv);
    }
}
```

Usar en todos lados:
```csharp
// CreateTransaccionHandler.cs
var impuesto = MoneyRounding.CalculateIgv(subtotal, tasaIgv);

// SunatOseClient.cs
var igvLinea = MoneyRounding.CalculateIgv(item.SubtotalLinea, tasa);
```

**Verificable:** Test que compare total local vs respuesta de Nubefact en casos con `.125`:
```csharp
[Fact]
public void MoneyRounding_MidpointValue_RoundsAwayFromZero()
{
    var result = MoneyRounding.Round(0.125m);
    result.Should().Be(0.13m);
}
```

---

### F-06: PIN en texto plano en el seed

**Archivo:** `src/CafeBarrio.Infrastructure/Persistence/Seeders/CatalogDataSeeder.cs` (línea ~141, verificar)

**Problema:** Si el seed persiste un operador con `PinHash = "123456"` (sin hashear), ese PIN queda conocido.

**Acción correctiva:** Eliminar credenciales del seed o hashearlas:
```csharp
// En CatalogDataSeeder
if (!_db.Operadores.Any())
{
    // Opción A: No crear operador en seed (debe crearse manualmente en prod)
    // Opción B: Hashear
    var hashedPin = _hasher.Hash("123456");  // Espera IPasswordHasher inyectado
    _db.Operadores.Add(new Operador
    {
        Nombre = "Demo Operador",
        PinHash = hashedPin,
        // ...
    });
}
```

O, preferible: **condicionar el seed a entornos no-prod:**
```csharp
if (app.Environment.IsDevelopment())
{
    await seeder.SeedAsync();
}
```

**Verificable:** Inspección: no debe haber `PinHash = "123456"` ni variables de entorno con credenciales.

---

## 🟠 Hallazgos Altos (P1 — Próximo sprint)

| ID | Hallazgo | Acción |
|----|----------|--------|
| **F-07** | Anulación: `ConcurrencyException` no capturada | `BeginTransaction` + `catch` → error de negocio |
| **F-08** | `DeleteProductoHandler` retorna `Success` falso | Capturar `DbUpdateException`, loguear, re-lanzar |
| **F-09** | `MovimientoCaja` cascade delete de `Turno` | `DeleteBehavior.Restrict` + migración |
| **F-10** | Token de Operador no revocable | Incluir `security_stamp` en token |
| **F-11** | IDOR: sin validación de `SedeId` | `sede_id` en token + filtro en handlers |
| **F-12** | Credenciales admin en body de anulación | Eliminar; usar rol JWT |
| **F-13** | Conflicto `CreatedAt` (GETUTCDATE vs interceptor) | Dejar solo interceptor |
| **F-14** | Sin tests de idempotencia/SUNAT/concurrencia | Agregar 3 grupos de tests |

---

## ✅ Positivos que mantener

- ✅ Argon2id (128 MiB, OWASP 2026) + `FixedTimeEquals`
- ✅ Lockout de Admin (5 intentos / 15 min)
- ✅ Headers de seguridad completos (X-Frame-Options, CSP, HSTS)
- ✅ CORS explícito (sin `AllowAnyOrigin`)
- ✅ RowVersion en Producto (previene race conditions)
- ✅ Índice único filtrado en Turno (abierto)
- ✅ Transacción explícita en ventas + captura de `ConcurrencyException`
- ✅ Health checks para BD y SUNAT
- ✅ Serilog JSON + CorrelationId
- ✅ CI/CD con tests de integración en SQL Server real
- ✅ Gobernanza MUIS excelente (Ledger, ADR, Guardrails)

---

## 📋 Plan de Ejecución

### Iteración Actual (P0 — 3–5 días)

1. **F-01:** Arreglar idempotencia (dos SaveChanges o FK real)
2. **F-02:** Sacar clave JWT, ampliar placeholders, exigir en todos los entornos
3. **F-03:** TrustedProxyIPs + lockout de Operador (5 fallos / 15 min)
4. **F-04:** EnableRetryOnFailure + CommandTimeout + pool + execution strategy
5. **F-05:** `MoneyRounding` helper + cálculo por línea
6. **F-06:** Eliminar/hashear PIN del seed

**Bloqueante:** cerrar P0 **antes de cualquier despliegue**.

### Sprint 1 (P1 — 1–2 semanas)

7. **F-07 a F-14:** Anulación, DeleteProducto, MovimientoCaja, Operador token, IDOR, credenciales, timestamps, tests

### Sprint 2+ (P2/P3 — deuda técnica)

- Índices faltantes
- Backoff SUNAT
- Unificar seeding
- Enums para estados
- OpenTelemetry
- Documentación de runbook

---

## 🎯 Veredicto

**Estado actual:** `NO APTO PARA PRODUCCIÓN`  
**Bloquea:** F-01 a F-06  
**Tras resolver P0:** `APTO PARA PRODUCCIÓN` (sujeto a validación de comprobante SUNAT real)

**Ruta:** Aplicar P0 → Ejecutar tests verificables → Green CI → Desplegar staging → Validar ticket SUNAT real → Producción.

---

*Documento generado conforme al protocolo MUIS_DEVOPS. Para Task Bundles B detallados, solicita generación de bundles P0 específicos.*
