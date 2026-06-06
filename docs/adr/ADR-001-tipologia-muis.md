# ADR-001: Clasificación MUIS — Café de Barrio

**Estado:** ACEPTADO
**Fecha de aceptación:** 2026-06-05
**Proyecto:** Café de Barrio — Cloud-Backed POS
**Autor:** Pablo Joel Castillo Flores
**Referencia:** Domain Charter v1.0.0-SEALED-2026-06-05

---

## Contexto

Café de Barrio es un sistema POS cloud-backed para una cafetería de barrio en Perú. Actores: barista (registra ventas) y dueño (revisa reportes). Volumen operacional estimado: <10 usuarios activos, <100 transacciones diarias, una sola sede.

La elección de tipología MUIS es la decisión arquitectónica más importante del proyecto. Determina qué módulos se activan, con qué intensidad, y en qué orden. Una tipología incorrecta invalida todas las decisiones que le siguen (Vector F-11, severidad ■□□ en Tipo 1).

**Resultado del Cuestionario de Clasificación MUIS:**

| Dimensión | Opción | Puntos | Justificación |
|---|---|---|---|
| D1 Escala de usuarios | A | 1 | ≤10 usuarios — barista + dueño en una sede |
| D2 Disponibilidad y SLA | A | 1 | Uso interno, best effort, sin SLA contractual |
| D3 Criticidad de datos | B | 2 | Transacciones financieras — pérdida recuperable con backup diario |
| D4 Tamaño del equipo | A | 1 | 1 desarrollador |
| D5 Frecuencia de updates | A | 1 | Actualizaciones por milestone — aprox. mensual |
| D6 Complejidad integraciones | A | 1 | Sin integraciones externas en v1 |
| D7 Distribución geográfica | A | 1 | Una sede — Perú |
| D8 Entorno regulatorio | A | 1 | Ley 29733 mínimo impacto — sin regulación crítica |
| **TOTAL** | | **9/32** | |

Override Tipo 4: NO — ningún indicador de IoT, ML, HFT o sistemas de seguridad física.

---

## Decisión

**Tipología asignada: TIPO 1 — Monolítico / Single-Service**

Con 9/32 puntos, el sistema está claramente dentro del rango Tipo 1 (8-14). Las dimensiones determinantes son D1 (escala mínima), D4 (equipo individual) y D6 (sin integraciones). D3=2 por la naturaleza de transacciones financieras, pero sin SLA contractual ni volumen multi-sede, Tipo 1 es la clasificación correcta.

---

## Consecuencias

### Positivas — lo que esta tipología habilita

- Arquitectura monolítica Clean Architecture sin microservicios — máxima velocidad de desarrollo para equipo individual
- MUIS_GATEWAY excluido — no requiere routing complejo con 1 servicio
- MUIS_MESSAGING excluido — sin comunicación asíncrona necesaria
- Stack simple: API + BD + Cliente = 3 componentes máximo
- Sin CI/CD obligatorio en v1 — despliegue manual aceptable

### Negativas — restricciones aceptadas

- Sin escalado horizontal — si el negocio abre segunda sede, re-clasificar
- Sin SLA contractual — down time aceptable durante mantenimiento
- Sin Circuit Breaker, Retry policies avanzadas ni Chaos Engineering
- MUIS_AUDIT diferido — si se necesita en v2, requiere migración

### Triggers de re-evaluación

Revisar esta clasificación si ocurre cualquiera de los siguientes eventos:

□ El negocio abre una segunda sede activa con usuarios independientes
□ El sistema integra pagos digitales regulados (Izipay, Culqui, etc.)
□ Se firma un SLA contractual con el operador (uptime garantizado)
□ El equipo supera 2 personas activas en el proyecto
□ Se añade integración con sistema externo (SUNAT, API bancaria)
□ El volumen supera 1,000 transacciones diarias sostenidas

Si algún trigger se activa → crear ADR-002 con la re-evaluación formal. No modificar este ADR.

---

## Alternativas consideradas

| Tipología alternativa | Puntaje hipotético / Override | Razón de descarte |
|---|---|---|
| Tipo 2 | 15-24 hipotético | Over-engineering — no hay usuarios pagos, no hay multi-tenancy, no hay CI/CD obligatorio para equipo individual |
| Tipo 3 | >24 hipotético | Completamente desproporcionado — sin compliance, sin equipos distribuidos, sin SLA contractual |

---

## Módulos activados por esta tipología

Referencia completa en Domain Charter, Sección 15.

**Módulos activos (Tipo 1, intensidad mínima):** MUIS_CORE, MUIS_DATA_STORAGE, MUIS_BACKEND, MUIS_QA_TESTING, MUIS_DEVOPS

**Módulos diferidos (activar en fases posteriores):** MUIS_SECURITY_AUTH (Fase 3), MUIS_FRONTEND_WEB (Fase 3), MUIS_DESIGN_SYSTEM (Fase 3), MUIS_AUDIT (si S2=SÍ)

**Módulos excluidos:** MUIS_MESSAGING, MUIS_GOVERNANCE, MUIS_GATEWAY, MUIS_FRONTEND_MOBILE, MUIS_BFF, MUIS_NOTIFICATIONS, MUIS_AI_ANALYTICS, MUIS_WORKFLOWS, MUIS_PAYMENTS, MUIS_SEARCH, MUIS_PLATFORM_ENGINEERING

---

## Inmutabilidad

Este ADR es inmutable a partir de su aceptación. Registra la decisión tal como fue tomada, con la información disponible el 2026-06-05. Si las condiciones del proyecto cambian y requieren re-clasificación, crear ADR-002. Nunca modificar ADR-001.

**Domain Charter asociado:** `d:\Dev\CafeDeBarrio-POS\docs\domain_charter_v1.0.0-SEALED-2026-06-05.md`
