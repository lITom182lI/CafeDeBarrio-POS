# Domain Charter — Café de Barrio

**Versión:** v1.1.0
**Modo MUIS_INCEPTION ejecutado:** EXPRESS (aplicación retroactiva)
**Estado:** SEALED

---

## Sección 1 — Identidad del Proyecto

| Campo | Valor |
|---|---|
| Nombre del proyecto | Café de Barrio — Cloud-Backed POS |
| Operador / Responsable de negocio | Dueño/operador del café (S3 — confirmar nombre antes de Fase 3) |
| Responsable técnico | Pablo Joel Castillo Flores |
| Fecha de inicio de MUIS_INCEPTION | 2026-06-05 (retroactivo — proyecto iniciado antes de MUIS_INCEPTION) |
| Modo ejecutado | EXPRESS retroactivo |
| Versión del Charter | v1.1.0 |

---

## Sección 2 — Resumen Ejecutivo

Sistema POS cloud-backed para una cafetería de barrio en Perú. Resuelve el problema de gestión manual de ventas e inventario (papel/Excel) mediante una API REST con base de datos relacional. El sistema v1 entrega: base de datos con 12 entidades, API REST operativa, y en fases siguientes un dashboard web para el dueño y un cliente POS para el barista.

El operador objetivo es el dueño/barista de una cafetería pequeña (<50 usuarios). La solución opera en modo cloud-backed local: el servidor corre en la nube o en un equipo local accesible en red.

---

## Sección 3 — Actores y Canales

| Actor | Rol en el sistema | Canal de acceso | Frecuencia de uso |
|---|---|---|---|
| Barista / Operador | Registra ventas, consulta productos, gestiona stock | POS C# desktop (local) — Fase 4 | Diaria, alta frecuencia |
| Dueño / Administrador | Revisa reportes, analiza ventas, gestiona catálogo | Dashboard React web — Fase 3 | Diaria o semanal |
| Sistema API | Procesa todas las operaciones | HTTP REST (localhost:5138 en desarrollo) | Continua |

---

## Sección 4 — Modelo de Negocio e Ingresos

**Modelo de negocio del negocio servido:**
Cafetería de barrio — ingresos por ventas presenciales de café y productos. Transacciones en efectivo y/o POS físico externo.

**Modelo de ingresos del software:**

☑ Uso interno — sin ingresos propios del software

**Intensidad de MUIS_PAYMENTS determinada:** Excluido (sin pagos digitales regulados en v1)

---

## Sección 5 — Análisis As-Is

**Estado actual del negocio:**
Gestión manual o mediante Excel. Sin sistema digital para registro de ventas ni control de inventario en tiempo real. Cierre de caja manual al fin del día.

**Datos existentes:**
- Origen: ninguno (sistema nuevo)
- Volumen estimado: 0 registros a migrar
- Plan de migración: No requerida — datos nuevos desde v1

---

## Sección 6 — Entidades y Flujos del Dominio

### Entidades principales (Fases 1-2 implementadas)

| Entidad | Atributos clave identificados | Relaciones principales |
|---|---|---|
| Transaccion | TransaccionId, FechaHora, Total, IGV, Subtotal, MetodoPagoId, ClienteId, SedeId | → DetalleTransaccion, → MetodoPago, → Cliente, → Sede |
| DetalleTransaccion | DetalleId, TransaccionId, ProductoId, Cantidad, PrecioUnitario, Subtotal | → Transaccion, → Producto |
| Producto | ProductoId, Nombre, Precio, Stock, CategoriaId | → CategoriaCafe, → DetalleTransaccion |
| Cliente | ClienteId, Nombre, Email, TipoClienteId | → TipoCliente, → Transaccion |
| Sede | SedeId, Nombre, Direccion | → Transaccion |
| MetodoPago | MetodoPagoId, Nombre | → Transaccion |
| CategoriaCafe | CategoriaId, Nombre | → Producto |
| TipoCliente | TipoClienteId, Nombre | → Cliente |
| OpcionEnvio | OpcionEnvioId, Nombre, Costo | referenciado en v2 |
| UbicacionPreferencia | UbicacionId, Descripcion | referenciado en v2 |
| TransaccionMayorista | (entidad extendida para pedidos mayoristas) | independiente |
| Transporte | TransporteId, Nombre | referenciado en v2 |

### Flujo de eventos principal

Barista → [RegistrarVenta] → [TransaccionCreada] → DetalleTransaccion generados + Stock de Producto actualizado
Dueño → [ConsultarReportes] → [ReporteGenerado] → Datos de ventas por fecha/producto/método de pago

---

## Sección 7 — Mapa Regulatorio

| Regulación | ¿Aplica? | Impacto en el sistema |
|---|---|---|
| Ley 29733 — Protección datos personales (Perú) | SÍ | Cliente.Nombre y Cliente.Email requieren consentimiento implícito. Datos mínimos necesarios. |
| SUNAT — Facturación electrónica | SÍ (v1.1) | Integración completa implementada en el backend para emitir boletas. |
| PCI-DSS | NO | Sin procesamiento de pagos digitales en v1. |
| GDPR | NO | Operación local Perú, sin usuarios EU. |
| HIPAA | NO | No aplica al dominio. |

---

## Sección 8 — Restricciones Duras Aprobadas

| Restricción | Detalle confirmado |
|---|---|
| Presupuesto | Proyecto académico UPN — sin presupuesto monetario |
| Timeline | Sin fecha límite contractual — desarrollo por fases |
| Equipo | 1 desarrollador (Pablo Joel Castillo Flores) |
| Infraestructura | Docker Desktop en Windows 11 Pro local |
| Tecnología obligada | C# .NET 9, SQL Server 2022, React + TypeScript + Vite |

---

## Sección 9 — Resiliencia Offline

**¿Requiere funcionamiento offline?** PENDIENTE (Suposición S1 — ver Sección 16)

**Para v1 (Fases 1-2):** Cloud-first. El POS cliente llama a la API; si la API no responde, la venta no se registra.

**Fase 4 (POS C# cliente):** Evaluar si el barista necesita registrar ventas cuando el servidor está caído. Si S1=SÍ → activar caché local + sincronización posterior.

**Arquitectura resultante v1:** Cloud-first
**MUIS_FRONTEND offline-first activado:** NO en v1 — Evaluar en Fase 4

---

## Sección 10 — Multi-tenancy

**Modelo:** Single-tenant

Una sola sede de cafetería. Sin requerimiento de aislamiento de datos entre tenants.

- ITenantContext: NO activado
- Rate Limiting por tenant: NO aplicable
- Particionado: NO aplicable

---

## Sección 11 — Auditabilidad

**¿El sistema requiere historial de cambios en los datos?** PENDIENTE (Suposición S2 — ver Sección 16)

**Si S2 = SÍ — entidades que requieren audit trail:**
Transaccion, Producto, DetalleTransaccion (trazabilidad para cierre de caja y control de inventario)

**Estrategia MUIS_AUDIT seleccionada según tipología:**
☑ Tipo 1: EfCoreAuditInterceptor síncrono (en EF Core pipeline)

> ⚠️ Esta decisión afecta el esquema de base de datos. Como Fases 1-2 ya están completas con migraciones aplicadas, la activación del audit trail requiere una nueva migración. Confirmar S2 antes de la próxima migración.

---

## Sección 12 — Métricas de Éxito del Proyecto

| Métrica | Valor objetivo | Cuándo medirla |
|---|---|---|
| Transacciones registradas sin error | 100% | Primer mes post-Fase 4 |
| Tiempo de carga del Dashboard | < 2 segundos | Al completar Fase 3 |
| Adopción del barista | Operador usa el POS sin ayuda técnica | Primera semana post-Fase 4 |

**Criterio mínimo de MVP (Definition of Done del proyecto):**
El barista puede registrar una venta desde el POS y el dueño puede ver el reporte de esa venta en el Dashboard el mismo día — sin intervención técnica de ningún tipo.

---

## Sección 13 — MVP v1 y Roadmap de Evolución

### Scope v1 — Incluye

- [x] Base de datos SQL Server con 12 entidades — COMPLETO
- [x] API REST: GET /api/productos, POST /api/transacciones — COMPLETO
- [ ] Dashboard React: reportes de ventas y productos (S3 — ver Sección 16)
- [ ] POS C# cliente con HttpClient integrado

### Scope v1 — Excluye explícitamente

- [x] Facturación electrónica SUNAT — COMPLETO
- [ ] Pagos digitales (Yape, Plin, tarjeta)
- [ ] Multi-sede
- [ ] App móvil
- [ ] Notificaciones externas (email, push)
- [ ] Módulo de turnos / cierre de caja formal (diferido a v2)

### Roadmap

| Versión | Scope estimado | Timeline estimado |
|---|---|---|
| v1 MVP | Fases 1-4 (DB + API + Dashboard + POS cliente) | En curso |
| v2 | Pagos digitales, cierre de caja | Post-v1 |
| v3+ | Multi-sede, analytics avanzados | TBD |

**Triggers de re-evaluación de tipología MUIS:**
- El negocio abre una segunda sede activa → re-evaluar Tipo 1 → Tipo 2
- Se integran pagos digitales regulados (Izipay, Culqui) → activar MUIS_PAYMENTS
- Se firma un SLA contractual con el operador → re-evaluar Tipo 2

---

## Sección 14 — Pre-clasificación MUIS (Input directo a MUIS_CLASIFICACION)

| Dimensión | Opción | Puntos | Fuente | Justificación |
|---|---|---|---|---|
| D1 Escala de usuarios | A | 1 | A1 Bloque A | ≤10 usuarios activos — barista + dueño en una sede |
| D2 Disponibilidad y SLA | A | 1 | D1 Bloque D | Best effort — uso interno sin SLA contractual |
| D3 Criticidad de datos | B | 2 | D5 Bloque D | Transacciones financieras — pérdida recuperable con backup diario |
| D4 Tamaño del equipo | A | 1 | Restricciones S8 | 1 desarrollador |
| D5 Frecuencia de updates | A | 1 | Roadmap S13 | Actualizaciones por milestone (mensual aprox.) |
| D6 Complejidad integraciones | A | 1 | Research S7 | Sin integraciones externas en v1 |
| D7 Distribución geográfica | A | 1 | A5 Bloque A | Una sola sede — Perú |
| D8 Entorno regulatorio | A | 1 | Mapa S7 | Solo Ley 29733 — impacto mínimo en v1 |
| **TOTAL** | | **9/32** | | |

**Tipología estimada:** Tipo 1 — Monolítico / Single-Service

**Override Tipo 4:** NO

---

## Sección 15 — Módulos MUIS Pre-Activados

| Módulo | Estado | Intensidad / Modo | Justificación breve |
|---|---|---|---|
| MUIS_CORE | Activo | Mínimo | IRepository, Result, DomainException — base de Clean Architecture |
| MUIS_DATA_STORAGE | Activo | Mínimo | SQL Server Docker + EF Core 9 — único store |
| MUIS_MESSAGING | Excluido | N/A | Tipo 1 — sin comunicación asíncrona |
| MUIS_BACKEND | Activo | Mínimo | Clean Architecture + MediatR + FluentValidation |
| MUIS_SECURITY_AUTH | Diferido | Mínimo | JWT básico — activar en Fase 3 (Dashboard) |
| MUIS_GOVERNANCE | Excluido | N/A | Tipo 1 — equipo individual |
| MUIS_DESIGN_SYSTEM | Diferido | Mínimo | Design tokens básicos — activar en Fase 3 |
| MUIS_GATEWAY | Excluido | N/A | Single-service — no requiere gateway |
| MUIS_AUDIT | Diferido | Mínimo | EfCoreAuditInterceptor — activar si S2=SÍ (próxima migración) |
| MUIS_FRONTEND_WEB | Diferido | Mínimo | React + Vite Dashboard — Fase 3 |
| MUIS_FRONTEND_MOBILE | Excluido | N/A | No requerido en v1 |
| MUIS_BFF | Excluido | N/A | Single-client — no requiere BFF |
| MUIS_NOTIFICATIONS | Excluido | N/A | Sin notificaciones externas en v1 |
| MUIS_AI_ANALYTICS | Excluido | N/A | Tipo 1 — sin ML/BI avanzado |
| MUIS_WORKFLOWS | Excluido | N/A | Sin procesos background en v1 |
| MUIS_PAYMENTS | Excluido | N/A | Sin pagos digitales en v1 |
| MUIS_SEARCH | Excluido | N/A | Búsqueda SQL básica suficiente |
| MUIS_QA_TESTING | Activo | Mínimo | Integration tests contra BD real — sin mocks |
| MUIS_DEVOPS | Activo | Mínimo | Bundles de sesión, AI orchestration, handoffs |
| MUIS_PLATFORM_ENGINEERING | Excluido | N/A | Tipo 1 — sin infraestructura de plataforma |

---

## Sección 16 — Suposiciones y Risk Register Semilla

### Suposiciones documentadas

| # | Suposición | Riesgo | Plan de verificación |
|---|---|---|---|
| S1 | El POS C# cliente (Fase 4) requerirá modo offline para registrar ventas cuando el servidor no responde | Medio — afecta arquitectura Fase 4 | Confirmar con el operador antes de iniciar Fase 4 |
| S2 | El operador requiere audit trail en Transaccion y Producto (historial de cambios para control de caja) | Alto — afecta esquema DB con nueva migración | Confirmar antes de iniciar la próxima migración de EF Core |
| S3 | El scope del Dashboard Fase 3 incluye: reportes de ventas diarias por método de pago, inventario de productos, y top 5 productos más vendidos | Bajo — cambio de scope no afecta arquitectura | Confirmar con el operador al inicio de Fase 3 |

### Risk Register Semilla — Tipo 1

| Vector | Prob (1–5) | Impacto (1–5) | Tratamiento activo | Responsable | Última revisión |
|---|---|---|---|---|---|
| **F-09** Over-engineering | | | Revisar cada feature antes de implementarla: ¿el approach más simple no es suficiente? | Pablo JCF | 2026-06-05 |
| **F-01** Impuesto al boilerplate | | | Lógica de dominio real ejecutándose desde Fase 1 — ✅ cumplido | Pablo JCF | 2026-06-05 |
| **F-16** Desconexión Producto-Ingeniería | | | Demo con el operador real al iniciar cada fase. Decisiones de UX con el operador. | Pablo JCF | 2026-06-05 |
| **F-17** Monopolio de conocimiento | | | Documentar decisiones en CLAUDE.md del proyecto. ADR-001 como referencia activa. | Pablo JCF | 2026-06-05 |
| **F-11** Tipología incorrecta | | | Re-evaluar si el negocio abre segunda sede o integra pagos regulados. | Pablo JCF | 2026-06-05 |

---

## Sección 17 — Política de Cambios Post-Charter

> Todo cambio de alcance posterior al sellado de este Charter requiere un Change Request documentado con:
> 1. Descripción del cambio solicitado
> 2. Impacto en entidades y flujos del dominio (¿qué secciones del Charter cambian?)
> 3. Impacto en tipología MUIS (¿el cambio eleva la tipología estimada?)
> 4. Impacto en timeline y presupuesto estimado
> 5. Aprobación firmada del operador y del responsable técnico
>
> **Sin este proceso documentado y aprobado, no se implementa ningún cambio de alcance.**

---

## Sección 18 — SOW y DoD

No aplica — proyecto de uso interno / académico, modo EXPRESS. Sin cliente externo.

---

## Aprobación

| Rol | Nombre | Fecha | Firma / Confirmación escrita |
|---|---|---|---|
| Operador / Dueño del negocio | (S3 — confirmar nombre) | 2026-06-05 | Retroactivo — confirmar en inicio de Fase 3 |
| Responsable técnico | Pablo Joel Castillo Flores | 2026-06-05 | Confirmado |

**Versión final sellada:** `v1.0.0-SEALED-2026-06-05`
