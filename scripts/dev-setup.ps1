#Requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Cafe de Barrio POS - Dev Setup ===" -ForegroundColor Cyan

# 1. Verificar prerrequisitos
Write-Host "`n[1/6] Verificando prerrequisitos..." -ForegroundColor Yellow

$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion -or -not ($dotnetVersion -match "^9\.")) {
    Write-Error ".NET 9 SDK requerido. Instalar desde https://dot.net/9"
}
Write-Host "  .NET $dotnetVersion" -ForegroundColor Green

$nodeVersion = node --version 2>$null
if (-not $nodeVersion) {
    Write-Error "Node.js requerido. Instalar desde https://nodejs.org"
}
Write-Host "  Node $nodeVersion" -ForegroundColor Green

docker info 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Docker no esta corriendo. Iniciar Docker Desktop."
}
Write-Host "  Docker OK" -ForegroundColor Green

# 2. SQL Server dev
Write-Host "`n[2/6] Levantando SQL Server dev..." -ForegroundColor Yellow
docker compose -f docker-compose.dev.yml up -d
if ($LASTEXITCODE -ne 0) { Write-Error "No se pudo levantar SQL Server dev" }
Write-Host "  Contenedor iniciado" -ForegroundColor Green

# 3. appsettings.Development.json
Write-Host "`n[3/6] Configurando API..." -ForegroundColor Yellow
$settingsPath = "src/CafeBarrio.API/appsettings.Development.json"
if (-not (Test-Path $settingsPath)) {
    $template = Get-Content "src/CafeBarrio.API/appsettings.Development.template.json" -Raw
    $jwtKey = [Convert]::ToBase64String(
        [byte[]](1..64 | ForEach-Object { [System.Security.Cryptography.RandomNumberGenerator]::GetInt32(256) })
    )
    $template = $template -replace "DEV_JWT_KEY_PLACEHOLDER", $jwtKey
    Set-Content $settingsPath $template -Encoding UTF8
    Write-Host "  appsettings.Development.json creado" -ForegroundColor Green
} else {
    Write-Host "  appsettings.Development.json ya existe, sin cambios" -ForegroundColor Gray
}

# 4. .env de frontends
Write-Host "`n[4/6] Configurando frontends..." -ForegroundColor Yellow

if (-not (Test-Path "dashboard/.env")) {
    Set-Content "dashboard/.env" "VITE_API_URL=http://localhost:5138`nVITE_SEDE_ID=1" -Encoding UTF8
    Write-Host "  dashboard/.env creado" -ForegroundColor Green
} else {
    Write-Host "  dashboard/.env ya existe, sin cambios" -ForegroundColor Gray
}

if (-not (Test-Path "pos-pwa/.env")) {
    Set-Content "pos-pwa/.env" "VITE_API_URL=http://localhost:5138`nVITE_SEDE_ID=1`nVITE_SENTRY_DSN=" -Encoding UTF8
    Write-Host "  pos-pwa/.env creado" -ForegroundColor Green
} else {
    Write-Host "  pos-pwa/.env ya existe, sin cambios" -ForegroundColor Gray
}

# 5. Restaurar dependencias
Write-Host "`n[5/6] Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore src/CafeBarrio.sln
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet restore fallo" }

Push-Location dashboard
npm install --silent
if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Error "npm install en dashboard fallo" }
Pop-Location

Push-Location pos-pwa
npm install --silent
if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Error "npm install en pos-pwa fallo" }
Pop-Location

Write-Host "  Dependencias restauradas" -ForegroundColor Green

# 6. Migraciones EF Core
Write-Host "`n[6/6] Aplicando migraciones..." -ForegroundColor Yellow

$retries = 0
$maxRetries = 24
do {
    $containerId = docker compose -f docker-compose.dev.yml ps -q sqlserver-dev 2>$null
    if ($containerId) {
        $health = docker inspect -f '{{.State.Health.Status}}' $containerId 2>$null
    } else {
        $health = "starting"
    }
    if ($health -ne "healthy") {
        Write-Host "  Esperando SQL Server ($retries/$maxRetries)..." -ForegroundColor Gray
        Start-Sleep 5
        $retries++
    }
} while ($health -ne "healthy" -and $retries -lt $maxRetries)

if ($health -ne "healthy") {
    Write-Error "SQL Server no respondio en tiempo. Verificar Docker."
}

dotnet tool install --global dotnet-ef 2>$null
dotnet ef database update `
    --project src/CafeBarrio.Infrastructure `
    --startup-project src/CafeBarrio.API
if ($LASTEXITCODE -ne 0) { Write-Error "EF migrations fallaron" }
Write-Host "  Migraciones aplicadas" -ForegroundColor Green

Write-Host "`n=== Setup completado ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para desarrollar:" -ForegroundColor White
Write-Host "  API:       dotnet run --project src/CafeBarrio.API" -ForegroundColor White
Write-Host "  Dashboard: cd dashboard && npm run dev  -> http://localhost:5173" -ForegroundColor White
Write-Host "  POS:       cd pos-pwa && npm run dev    -> http://localhost:5174" -ForegroundColor White
Write-Host ""
Write-Host "Para detener SQL Server dev:" -ForegroundColor White
Write-Host "  docker compose -f docker-compose.dev.yml down" -ForegroundColor White
