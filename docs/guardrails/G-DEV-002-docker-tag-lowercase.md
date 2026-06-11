---
id: G-DEV-002
type: anti-pattern
scope: MUIS-CORE
layer: devops
trigger: Docker, tag, imagen, uppercase, lowercase, github.repository_owner, ghcr, registry
linked-ledger: PROD-04
linked-adr: ""
last-reviewed: 2026-06-11
review-when: Si se migra a otro registry de imágenes Docker
---

**Regla:** Nunca usar `github.repository_owner` directamente en tags de imágenes Docker — siempre normalizar a lowercase mediante `tr '[:upper:]' '[:lower:]'`.

**Why:** Docker rechaza tags con caracteres uppercase. `github.repository_owner` devuelve el username de GitHub con el case original del registro. El CI fallaba con `invalid tag "ghcr.io/lITom182lI/cafebarrio-api:latest": repository name must be lowercase`. Descubierto en el job `docker-publish` de `ci.yml`.

**How to apply:**

```yaml
# CORRECTO: paso de normalización antes de usar el tag
- name: Derive lowercase image name
  run: echo "IMAGE=ghcr.io/$(echo '${{ github.repository_owner }}' | tr '[:upper:]' '[:lower:]')/nombre-api" >> $GITHUB_ENV

- name: Build and push
  uses: docker/build-push-action@v6
  with:
    tags: |
      ${{ env.IMAGE }}:latest
      ${{ env.IMAGE }}:${{ github.sha }}

# INCORRECTO: usar owner directamente
tags: ghcr.io/${{ github.repository_owner }}/nombre-api:latest
```
