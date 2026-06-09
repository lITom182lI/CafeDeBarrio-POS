// ============================================================
// MUIS_CORE — Capa de Contratos y Primitivos Compartidos
// Archivo: IBackgroundTenantContext.cs
// Versión: 1.0 | Junio 2026
// ============================================================
//
// PROPÓSITO GLOBAL:
//   Contrato mutable de resolución de identidad de inquilino para procesos
//   en segundo plano (Background Services, Kafka Consumers, CDC Handlers).
//
//   PROBLEMA QUE RESUELVE:
//   Los Consumers de Kafka (MUIS_MESSAGING) o los Handlers de CDC
//   (MUIS_AI_ANALYTICS con Debezium) no tienen HttpContext. El envelope
//   de Debezium NO incluye TenantId automáticamente — el Consumer debe
//   extraerlo del payload del evento CDC y establecerlo manualmente
//   antes de procesar cualquier operación con EF Core.
//
//   SIN esta interfaz, los Global Query Filters de EF Core aplicarían
//   un TenantId nulo, causando DataLeaks entre tenants.
//
// ACTIVACIÓN POR TIPOLOGÍA:
//   Tipo 1 ❌ — No aplica (sin Event Bus ni CDC).
//   Tipo 2 ⚠️ — Opcional. Activar si hay Workers background multi-tenant (Hangfire consumers).
//               No requerido si todos los jobs son single-tenant o el tenant es constante.
//   Tipo 3 ✅ — Kafka Consumers en Workers .NET 9.
//   Tipo 4 ✅ — IoT Event Processors de alta frecuencia.
//
// IMPLEMENTACIÓN:
//   Registrar como Scoped (¡nunca Singleton!):
//   builder.Services.AddScoped<IBackgroundTenantContext, BackgroundTenantContext>();
//   builder.Services.AddScoped<ITenantContext>(sp =>
//       sp.GetRequiredService<IBackgroundTenantContext>());
//
// PATRÓN DE USO EN UN KAFKA CONSUMER:
//   var tenantId = ExtractTenantFromCdcEnvelope(message);
//   _tenantContext.SetTenant(tenantId);
//   try { await _handler.HandleAsync(message); }
//   finally { _tenantContext.Clear(); } // Siempre limpiar al finalizar
// ============================================================

namespace MUIS_CORE.Interfaces;

/// <summary>
/// Extensión mutable de ITenantContext para procesos sin HttpContext.
/// Diseñada exclusivamente para Workers de Kafka, CDC Handlers y Background Jobs.
/// </summary>
public interface IBackgroundTenantContext : ITenantContext
{
    /// <summary>
    /// Establece el TenantId activo para el ciclo de procesamiento actual.
    /// Llamar antes de cualquier operación que involucre EF Core o Repositorios.
    /// </summary>
    /// <param name="tenantId">
    ///     ID del inquilino extraído del envelope CDC de Debezium
    ///     o del header del mensaje de Kafka.
    ///     Formato: string no vacío. Ejemplo: "tenant-acme", "org-12345".
    /// </param>
    /// <exception cref="ArgumentNullException">Si tenantId es nulo o vacío.</exception>
    void SetTenant(string tenantId);

    /// <summary>
    /// Limpia el TenantId al finalizar el procesamiento del mensaje.
    /// OBLIGATORIO llamar en un bloque finally para prevenir contaminación
    /// de contexto entre mensajes procesados en el mismo Worker thread.
    /// </summary>
    void Clear();
}
