#Requires -Version 5.1
<#
.SYNOPSIS
    Script de primera instalacion - Cafe de Barrio POS
    Uso: .\deploy\setup.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Cafe de Barrio POS - Instalacion ===" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Docker no esta instalado." -ForegroundColor Red
    Write-Host "Descarga Docker Desktop desde: https://www.docker.com/products/docker-desktop"
    exit 1
}
Write-Host "V Docker detectado: $(docker --version)" -ForegroundColor Green

# 2. Crear .env desde template si no existe
if (Test-Path ".env") {
    Write-Host "V .env ya existe - omitiendo creacion." -ForegroundColor Yellow
} else {
    if (-not (Test-Path ".env.template")) {
        Write-Host "ERROR: No se encontro .env.template en el directorio actual." -ForegroundColor Red
        exit 1
    }
    Copy-Item ".env.template" ".env"

    # Generar JWT Key automaticamente
    $jwtKey = [Convert]::ToBase64String(
        (1..64 | ForEach-Object { [byte](Get-Random -Max 256) })
    )
    (Get-Content ".env") -replace "<GENERADO_POR_SETUP>", $jwtKey |
        Set-Content ".env" -Encoding UTF8

    Write-Host "V .env creado con JWT Key generado automaticamente." -ForegroundColor Green
    Write-Host ""
    Write-Host "ACCION REQUERIDA: Edita .env y reemplaza los valores <CAMBIAR_...>" -ForegroundColor Yellow
    Write-Host "  - SA_PASSWORD   : contrasena para SQL Server"
    Write-Host "  - ADMIN_PASSWORD: contrasena del usuario admin del POS"
    Write-Host ""
    Write-Host "Cuando termines, ejecuta: docker compose up -d" -ForegroundColor Cyan
    exit 0
}

# 3. Verificar que el usuario haya configurado el .env
$envContent = Get-Content ".env" -Raw
if ($envContent -match "<CAMBIAR_") {
    Write-Host "ERROR: El archivo .env aun tiene valores sin configurar (<CAMBIAR_...>)." -ForegroundColor Red
    Write-Host "Edita .env antes de continuar."
    exit 1
}

# 4. Levantar contenedores
Write-Host "Iniciando contenedores..." -ForegroundColor Cyan
docker compose up -d

Write-Host ""
Write-Host "=== Instalacion completada ===" -ForegroundColor Green
Write-Host "API:          http://localhost:8080"
Write-Host "Health check: http://localhost:8080/health"
Write-Host ""
Write-Host "Primeros pasos:"
Write-Host "  1. Abre http://localhost:8080/health - debe mostrar 'Healthy'"
Write-Host "  2. Inicia el Dashboard con: cd dashboard && npm run dev"
Write-Host "  3. Login con admin@cafedebarrio.com y tu ADMIN_PASSWORD"
