---
id: G-DEV-001
type: anti-pattern
scope: MUIS-CORE
layer: devops
trigger: Antigravity, reemplazar, sobreescribir, archivo, contenido completo, append
linked-ledger: ""
linked-adr: ""
last-reviewed: 2026-06-11
review-when: Si se cambia el motor de ejecución de Antigravity
---

**Regla:** Al instruir a Antigravity que reemplace el contenido completo de un archivo, escribir explícitamente **"sobreescribir (no append)"** en la instrucción.

**Why:** Antigravity tiende a agregar el contenido nuevo al final del archivo existente en lugar de reemplazarlo. En TASK-06 esto produjo un `ci.yml` con dos bloques `name:`, `on:` y `jobs:` — YAML inválido que rompió el pipeline CI completo. La corrección requirió un TASK-07 completo de reescritura.

**How to apply:** En el campo "Tu tarea" o "Output esperado" del Bundle B, cuando se reemplaza un archivo completo:

```
CORRECTO:
"Sobreescribir (no append) el contenido completo de ci.yml con:"
"Reemplazar Program.cs — sobreescribir, no append:"

INCORRECTO (ambiguo — Antigravity hará append):
"Actualiza el archivo ci.yml con el siguiente contenido:"
"Escribe esto en Program.cs:"
```
