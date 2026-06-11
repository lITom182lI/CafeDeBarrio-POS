# TASK BUNDLE B — [nombre-tarea]
**Generado por: Claude Code | Uso: pegar después del SESSION_BUNDLE_A.md**

> Instrucción: Este es el Bundle B. Asume que ya recibiste el Bundle A.
> Ejecuta esta tarea exactamente como se describe. No agregues features fuera del scope.

## Identificación
| Campo | Valor |
|---|---|
| ID de tarea | TASK-[N]-[nombre-corto] |
| Fecha | YYYY-MM-DD |
| Módulo MUIS | MUIS_BACKEND / MUIS_FRONTEND_WEB / MUIS_DEVOPS / etc. |
| Tier de complejidad | 0 / 1 / 2 |
| Modelo destino | Gemini 2.5 Flash / Gemini 2.5 Pro |
| Plataforma | Antigravity |

## Tu tarea
[Descripción exacta en 1-3 oraciones.]

## Pre-flight — Verificación de guardrails
Antes de proponer la solución, verificar en `docs/guardrails/INDEX.md`:
- [ ] ¿La solución propuesta colisiona con algún guardrail de la capa afectada?
- [ ] ¿Existe un ADR en `docs/adr/INDEX.md` con precedente relevante?

Si hay colisión: notificar antes de proceder. No continuar sin confirmación.

## Inputs — Código y contexto de entrada
[Solo los archivos relevantes. No volcar el proyecto completo.]

## Restricciones para esta tarea
- [Qué NO debe cambiar]
- [Qué patrones MUIS debe respetar]
- [Qué NO debe agregar fuera del scope]
- Sobreescribir (no append) cuando se reemplace contenido completo de un archivo

## Output esperado
**Formato:** [código C# / TS / SQL / YAML / etc.]
**Archivos a generar/modificar:** [rutas relativas]

## Criterio de aceptación
✓ [condición 1]
✓ [condición 2]
✓ [condición 3]

## Notas adicionales
[Contexto arquitectónico relevante. Dejar en blanco si no aplica.]

---
> Cuando termines, entrega solo el código/output pedido.
> No expliques qué hiciste a menos que encuentres una ambigüedad.

## Output obligatorio — VALIDATION REPORT
Al terminar, genera y entrega este reporte en formato exacto:

```
TASK-[id] | [PASSED / FAILED] | [YYYY-MM-DD]
Build: [N err] [N warn]
Criterios: [✓ todos] o listar solo los ✗
Archivos: [N creados / modificados / eliminados]
Desviaciones: Ninguna / [descripción breve]
Decisiones propias: Ninguna / [descripción breve]
Spot-check: No requerido / [archivo:línea — motivo]
Guardrail: Ninguno / [G-XXX-NNN: crear | actualizar — descripción breve]
ADR: Ninguno / [ADR-NNN: título — crear | actualizar]
```

La tarea no está completa hasta que este reporte esté generado.
