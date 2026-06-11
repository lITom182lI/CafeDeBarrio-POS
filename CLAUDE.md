# Cafe de Barrio POS

## Clasificacion MUIS
Tipo 2 (SaaS Escalable)

## Stack Profile
- **Backend:** .NET 9, EF Core, MediatR (CQRS)
- **Frontend:** React (Vite), TypeScript, PWA
- **Base de Datos:** SQL Server (On-Premise / Azure SQL)

## Cloud Profile
- Entorno Híbrido: Despliegue Local (On-Premise) con proyección a Azure.

## Puertos
- **Backend API:** `http://localhost:5138` / `https://localhost:7240` (dev) · `http://+:8080` (Docker/producción)
- **Frontend Dashboard / POS:** `http://localhost:5173` / `http://localhost:5174`

## Notas de Gobernanza
*Evaluado bajo el ECOSISTEMA-MUIS. Ver `docs/MUIS_EVALUATION_LEDGER.md` para el estado actual de la deuda técnica y cumplimiento arquitectónico.*

*Memoria arquitectónica persistente: cargar `docs/MUIS_GUARDRAILS.md` al inicio de cada sesión. Para detalle por capa: `docs/guardrails/INDEX.md`. Para decisiones con rationale: `docs/adr/INDEX.md`.*

## Protocolo de Orquestación IA (MUIS_DEVOPS)
- **Claude Code** actúa como Arquitecto/Revisor: evalúa, genera instrucciones, valida. **No ejecuta código directamente.**
- Toda instrucción al modelo ejecutor se emite como **Task Bundle B** (ver `MUIS_DEVOPS/bundles/TASK_BUNDLE_TEMPLATE.md`).
- Flujo obligatorio: Claude genera Bundle B → usuario ejecuta en Antigravity → trae Validation Report → Claude valida.
- El Validation Report es obligatorio al final de cada tarea. La tarea no está completa sin él.
- Excepción única: si el usuario pide explícitamente que Claude ejecute algo puntual, se hace solo para esa tarea.
