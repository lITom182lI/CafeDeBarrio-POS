---
id: G-DEV-004
type: validated-decision
scope: TIPO-2
layer: devops
trigger: appsettings, Development, gitignore, template, onboarding, clonar, desarrollador, JWT
linked-ledger: DEVX-01
linked-adr: ADR-006
last-reviewed: 2026-06-11
review-when: Si se agregan nuevas claves requeridas en appsettings.Development.json
---

**Regla:** `appsettings.Development.json` debe estar en `.gitignore`. Mantener siempre `appsettings.Development.template.json` versionado con el placeholder `DEV_JWT_KEY_PLACEHOLDER`. El script `scripts/dev-setup.ps1` copia el template y genera la clave JWT automáticamente.

**Why:** `appsettings.Development.json` contiene configuración específica de cada máquina. Si se versiona, los devs sobreescriben la config de los demás. Si se gitignora sin template, un dev que clona no tiene referencia de qué configurar y el API no arranca. Con el patrón template + setup script, `git clone` + `.\scripts\dev-setup.ps1` produce un entorno funcionando sin intervención manual. Implementado en TASK-17 para equipo de 3 personas.

**How to apply:** Al agregar una nueva clave a `appsettings.Development.json`:
1. Agregar el campo con placeholder en `appsettings.Development.template.json`
2. Actualizar `scripts/dev-setup.ps1` si la clave requiere generación automática
3. Nunca commitear `appsettings.Development.json` directamente — está en `.gitignore` por diseño
