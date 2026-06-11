---
id: G-DEV-003
type: anti-pattern
scope: TIPO-2
layer: devops
trigger: Dockerfile, dotnet restore, .sln, solución, build context, tests, dockerignore
linked-ledger: PROD-03
linked-adr: ""
last-reviewed: 2026-06-11
review-when: Si se agrega un nuevo proyecto a la solución o se modifica el .dockerignore
---

**Regla:** En el `Dockerfile`, usar `dotnet restore NombreApi/NombreApi.csproj` — nunca `dotnet restore NombreApi.sln`.

**Why:** La solución `.sln` referencia proyectos de tests con rutas relativas fuera del build context de Docker (`"..\tests\CafeBarrio.Tests.Unit\..."`). El `.dockerignore` excluye esos proyectos para mantener la imagen liviana. Al restaurar la `.sln`, Docker no puede resolver las rutas relativas y falla con `MSBUILD : error MSB1009`. Descubierto en TASK-08.

**How to apply:**

```dockerfile
# CORRECTO: restaurar solo el proyecto de la API
COPY src/ .
RUN dotnet restore CafeBarrio.API/CafeBarrio.API.csproj
RUN dotnet publish CafeBarrio.API/CafeBarrio.API.csproj -c Release -o /app/publish --no-restore

# INCORRECTO: restaurar la solución completa
RUN dotnet restore CafeBarrio.sln  # falla si .dockerignore excluye proyectos de tests
```

Verificar este guardrail si se modifica el `.dockerignore` o si se agrega un nuevo proyecto a la solución.
