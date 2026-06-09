// ============================================================
// MUIS_CORE — Capa de Contratos y Primitivos Compartidos
// Archivo: ITenantContext.cs
// Versión: 1.0 | Junio 2026
// ============================================================
//
// PROPÓSITO GLOBAL:
//   Contrato universal de resolución de identidad de inquilino (Tenant).
//   Aplica a CUALQUIER sistema multi-inquilino construido bajo MUIS,
//   independientemente del dominio de negocio.
//
// ACTIVACIÓN POR TIPOLOGÍA:
//   Tipo 1 ❌ — Single-tenant; no aplica.
//   Tipo 2 ✅ — SaaS con múltiples clientes en BD compartida.
//   Tipo 3 ✅ — Enterprise; aislamiento lógico + posible DB-per-tenant.
//   Tipo 4 ✅ — IoT/Real-Time; aislamiento de streams por tenant.
//
// IMPLEMENTACIÓN:
//   - Este contrato vive en la capa Application o Domain.
//   - La implementación HTTP vive en Infrastructure/Persistence/TenantContext.cs.
//   - Para procesos background (Kafka, Workers), usar IBackgroundTenantContext.
//
// USO:
//   builder.Services.AddScoped<ITenantContext, TenantContext>();
//
// RESOLUCIÓN DE TENANT (prioridad decreciente):
//   1. JWT Claim: `{TENANT_CLAIM_NAME}` (configurable por entorno)
//   2. HTTP Header: `X-Tenant-Id` (inyectado por MUIS_GATEWAY)
//   3. Subdominio: `{tenant}.{domain}.com` (Tipo 3+)
//   4. Exception: TenantResolutionException (nunca silenciar)
// ============================================================

namespace MUIS_CORE.Interfaces;

/// <summary>
/// Contrato de resolución de contexto del inquilino activo.
/// Inyectar como Scoped en el pipeline HTTP.
/// NUNCA usar Singleton — el tenant varía por request.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Identificador único del inquilino activo en el request actual.
    /// Immutable una vez resuelto en el pipeline HTTP.
    /// Lanzar TenantResolutionException si no puede resolverse.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Indica si el contexto de tenant fue resuelto exitosamente.
    /// Usar para validar en endpoints que soportan modo single-tenant
    /// (IsResolved = false en Tipo 1 sin multi-tenancy configurado).
    /// </summary>
    bool IsResolved { get; }
}
