# Risk Register — Café de Barrio

**Tipología:** Tipo 1 — Monolítico / Single-Service
**Inicializado:** 2026-06-05
**Fuente:** MUIS_INCEPTION/templates/risk_register_seed.md — Tipo 1
**Revisión:** Al inicio de cada milestone

---

## Vectores de alta prioridad (■■■) — Tipo 1

*Riesgo principal de Tipo 1: over-engineering (F-09). Vigilancia activa en cada decisión técnica.*

| Vector | Descripción del riesgo | Prob (1–5) | Impacto (1–5) | Tratamiento activo | Responsable | Última revisión |
|---|---|---|---|---|---|---|
| **F-09** Over-engineering | Adoptar patrones de Tipo 3/4 (microservicios, event sourcing, CQRS complejo) en un problem que se resuelve con CRUD monolítico | 2 | 4 | Revisar cada feature antes de implementarla: ¿el approach más simple no es suficiente? | Pablo JCF | 2026-06-05 |
| **F-01** Impuesto al boilerplate | Sprint 0 y 1 terminan sin una sola línea de lógica de dominio ejecutándose | 1 | 3 | Lógica de dominio real ejecutándose desde Fase 1 — ✅ cumplido (POST /api/transacciones funcional) | Pablo JCF | 2026-06-05 |
| **F-16** Desconexión Producto-Ingeniería | Sistema técnicamente correcto con cero adopción del operador | 3 | 5 | Demo con el operador real al iniciar cada fase. Decisiones de UX con el operador, no sin él | Pablo JCF | 2026-06-05 |
| **F-17** Monopolio de conocimiento | Bus factor = 1 (solo Pablo conoce el sistema) | 5 | 3 | Documentar decisiones clave en CLAUDE.md del proyecto. ADR-001 como referencia activa. Domain Charter sellado. | Pablo JCF | 2026-06-05 |
| **F-11** Tipología incorrecta | Over-engineering por clasificación errónea (asumir Tipo 2 cuando es Tipo 1) | 2 | 4 | Re-evaluar tipología si se activa algún trigger del ADR-001 | Pablo JCF | 2026-06-05 |

---

## Vectores personalizados del proyecto

| Vector | Descripción del riesgo | Prob (1–5) | Impacto (1–5) | Tratamiento activo | Responsable | Última revisión |
|---|---|---|---|---|---|---|
| **F-CDB-01** Suposición S2 no resuelta | Si el operador necesita audit trail y se detecta en Fase 3+, la migración será destructiva | 3 | 4 | Confirmar S2 (audit trail) con el operador antes de la próxima migración de EF Core | Pablo JCF | 2026-06-05 |
| **F-CDB-02** Scope creep en Dashboard | El operador pide features fuera del scope v1 durante Fase 3 sin proceso formal | 4 | 3 | Aplicar Política de Cambios Post-Charter (Sección 17 del Charter). Change Request documentado. | Pablo JCF | 2026-06-05 |

## Suposiciones resueltas

| # | Suposición | Resolución | Fecha |
|---|---|---|---|
| S2 | Audit trail en Transaccion y Producto | **CONFIRMADO** — EfCoreAuditInterceptor implementado en TASK_S2_AUDIT_TRAIL. Columnas: CreatedAt + UpdatedAt en Transaccion y Producto. Migración: S2_AuditTrail. | 2026-06-05 |
| S4 | Tasa IGV aplicable | **CONFIRMADO** — Régimen general 18% (TasaIGV=0.16 + TasaIPM=0.02). Migración: S4_IgvRegimenGeneral18Pct. Confirmado por operador/contador. | 2026-06-05 |
