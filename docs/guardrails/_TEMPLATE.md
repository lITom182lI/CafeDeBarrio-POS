---
id: G-[CAPA]-[NNN]
type: anti-pattern          # anti-pattern | validated-decision | protected-zone
scope: PROYECTO             # PROYECTO | TIPO-2 | MUIS-CORE
layer: infrastructure       # infrastructure | security | devops | backend | frontend | domain
trigger: keyword1, keyword2 # palabras clave que activan consulta de este guardrail
linked-ledger: ""           # ID del Ledger relacionado (ej: F-05) o vacío
linked-adr: ""              # ID del ADR relacionado (ej: ADR-003) o vacío
last-reviewed: YYYY-MM-DD
review-when: ""             # condición de revisión (ej: "Upgrade EF Core 10+")
---

**Regla:** [Una oración prescriptiva — qué hacer o no hacer.]

**Why:** [Por qué existe esta regla. El incidente o razonamiento que la originó.
Qué pasó cuando se violó. Ser específico — archivo, línea, error si aplica.]

**How to apply:** [Cuándo verificar este guardrail. Cómo aplicarlo en la práctica.
Incluir snippet de código si aplica.]
