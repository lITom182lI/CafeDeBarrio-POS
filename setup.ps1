#Requires -Version 5.1
<#
.SYNOPSIS
    Cafe de Barrio POS - Setup inicial en cualquier PC.
    Genera .env con passwords seguros y verifica requisitos.

.USAGE
    .\setup.ps1
    .\setup.ps1 -Force   # Sobreescribe .env existente
#>
param([switch]$Force)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# -- Colores ----------------------------------------------------------------
function Write-Ok($msg)   { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  [!!] $msg" -ForegroundColor Yellow }
function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }

Write-Host "`n================================================" -ForegroundColor DarkCyan
Write-Host "   Cafe de Barrio POS - Setup"                      -ForegroundColor DarkCyan
Write-Host "================================================`n"  -ForegroundColor DarkCyan

# -- 1. Verificar requisitos ------------------------------------------------
Write-Step "Verificando requisitos"

$dockerOk = $null -ne (Get-Command docker -ErrorAction SilentlyContinue)
if ($dockerOk) { Write-Ok "Docker encontrado" }
else           { Write-Warn "Docker no encontrado - instalar desde https://docs.docker.com/desktop/windows/" }

$composeOk = $null -ne (Get-Command docker -ErrorAction SilentlyContinue) -and
             (docker compose version 2>$null)
if ($composeOk) { Write-Ok "Docker Compose disponible" }

# -- 2. Generar passwords seguros -------------------------------------------
Write-Step "Generando credenciales seguras"

function New-SecurePass([int]$len) {
    # Excluye $ para evitar interpolación en archivos .env de Docker Compose
    $chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@%^&*'
    do {
        $p = -join ((1..$len) | ForEach-Object { $chars[(Get-Random -Max $chars.Length)] })
    } while (-not ($p -cmatch '[A-Z]' -and $p -cmatch '[a-z]' -and
                   $p -match '[0-9]' -and $p -match '[!@%^&*]'))
    return $p
}

$envPath = Join-Path $PSScriptRoot '.env'

if ((Test-Path $envPath) -and -not $Force) {
    Write-Warn ".env ya existe. Usa -Force para sobreescribir."
    Write-Host "`n  El sistema esta listo. Ejecuta:" -ForegroundColor White
    Write-Host "    docker compose pull" -ForegroundColor White
    Write-Host "    docker compose up -d`n" -ForegroundColor White
    exit 0
}

$sa    = New-SecurePass 20
$jwt   = New-SecurePass 44
$admin = New-SecurePass 16

# -- 3. Escribir .env -------------------------------------------------------
$envContent = @"
SA_PASSWORD=$sa
JWT_KEY=$jwt
CORS_ORIGIN=http://localhost:5173,http://localhost:5174
ADMIN_PASSWORD=$admin
SUNAT_ENABLED=false
SUNAT_RUC=
SUNAT_OSE_TOKEN=
"@

Set-Content -Path $envPath -Value $envContent -Encoding utf8
Write-Ok ".env creado en $envPath"

# -- Crear .env.local para frontends (apuntan al API Docker en 8080) --------
$frontendEnv = "VITE_API_TARGET=http://127.0.0.1:8080"
Set-Content -Path (Join-Path $PSScriptRoot 'dashboard\.env.local')  -Value $frontendEnv -Encoding utf8
Set-Content -Path (Join-Path $PSScriptRoot 'pos-pwa\.env.local')    -Value $frontendEnv -Encoding utf8
Write-Ok "dashboard/.env.local y pos-pwa/.env.local creados (proxy -> Docker :8080)"

# -- 4. Mostrar credenciales generadas --------------------------------------
Write-Step "Credenciales generadas (guardalas en un lugar seguro)"
Write-Host ""
Write-Host "  SA_PASSWORD    : $sa"    -ForegroundColor White
Write-Host "  JWT_KEY        : $jwt"   -ForegroundColor White
Write-Host "  ADMIN_PASSWORD : $admin" -ForegroundColor White
Write-Host ""
Write-Warn "Estas credenciales NO se volveran a mostrar. Guardalas ahora."

# -- 5. Instrucciones siguientes --------------------------------------------
Write-Step "Siguiente paso - ejecuta estos comandos"
Write-Host ""
Write-Host "  docker compose pull" -ForegroundColor White
Write-Host "  docker compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "  API:       http://localhost:8080"              -ForegroundColor DarkGray
Write-Host "  Dashboard: http://localhost:5173"              -ForegroundColor DarkGray
Write-Host "  Login:     admin@cafedebarrio.com / $admin"  -ForegroundColor DarkGray
