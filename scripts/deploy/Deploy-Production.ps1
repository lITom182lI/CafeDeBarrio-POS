<#
.SYNOPSIS
    Despliega una nueva version de CafeDeBarrio al servidor de produccion.
.DESCRIPTION
    Flujo: backup -> migracion DB -> parar servicio -> copiar binarios -> iniciar servicio -> verificar health.
.EXAMPLE
    # Windows Auth (dev/staging):
    .\Deploy-Production.ps1 -Version "1.0.1" -WindowsAuth

    # SQL Auth (produccion tipica):
    .\Deploy-Production.ps1 -Version "1.0.1" -DbUser "cafebarrio_deploy" -DbPassword "Str0ng!"

    # Preview sin ejecutar:
    .\Deploy-Production.ps1 -Version "1.0.1" -DbUser "sa" -DbPassword "..." -WhatIf
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [string]$SourceDir        = "",
    [string]$ApiDestDir       = "C:\Services\CafeBarrioAPI",
    [string]$DashDestDir      = "C:\inetpub\wwwroot\cafebarrio-dashboard",
    [string]$PosDestDir       = "C:\inetpub\wwwroot\cafebarrio-pos",
    [string]$ServiceName      = "CafeBarrioAPI",
    [string]$HealthUrl        = "http://localhost:5000/health",

    [string]$DbServer         = "localhost",
    [string]$DbName           = "CafeDeBarrio",
    [string]$DbUser           = "",
    [string]$DbPassword       = "",
    [switch]$WindowsAuth,

    [switch]$SkipBackup,
    [switch]$SkipFrontend,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
if (-not $SourceDir) { $SourceDir = Join-Path (Split-Path -Parent (Split-Path -Parent $ScriptDir)) "release" }

$logDir  = Join-Path $ScriptDir "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
$logFile = "$logDir\deploy_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Log {
    param([string]$Msg, [string]$Color = "White")
    $line = "[$(Get-Date -Format 'HH:mm:ss')] $Msg"
    Write-Host $line -ForegroundColor $Color
    Add-Content -Path $logFile -Value $line
}

# --- Validaciones ---
Write-Log "==========================================" "Cyan"
Write-Log "  CafeDeBarrio Deploy v$Version" "Cyan"
Write-Log "  Source  : $SourceDir" "Cyan"
Write-Log "  WhatIf  : $WhatIf" "Cyan"
Write-Log "==========================================" "Cyan"

$apiSource   = "$SourceDir\api"
$sqlScript   = "$SourceDir\migrate-production.sql"
$dashSource  = "$SourceDir\dashboard"
$posSource   = "$SourceDir\pos-pwa"
$envFileDest = "$ApiDestDir\.env"

foreach ($req in @($apiSource, $sqlScript)) {
    if (-not (Test-Path $req)) { throw "Artefacto no encontrado: $req. Ejecutar build-release.ps1 primero." }
}
if (-not (Test-Path $envFileDest) -and -not $WhatIf) {
    throw ".env no encontrado en $envFileDest. Crear el archivo con los secretos de produccion antes de desplegar. Ver docs/runbook-produccion.md seccion 3."
}

if ($WhatIf) {
    Write-Log "[WhatIf] Pasos que se ejecutarian:" "Yellow"
    if (-not $SkipBackup) { Write-Log "  1. Backup de $DbName en $DbServer" "Yellow" }
    Write-Log "  2. Aplicar $sqlScript a $DbName" "Yellow"
    Write-Log "  3. Parar servicio $ServiceName" "Yellow"
    Write-Log "  4. Copiar $apiSource -> $ApiDestDir" "Yellow"
    if (-not $SkipFrontend) {
        if (Test-Path $dashSource) { Write-Log "  5. Copiar $dashSource -> $DashDestDir" "Yellow" }
        if (Test-Path $posSource)  { Write-Log "  6. Copiar $posSource -> $PosDestDir" "Yellow" }
    }
    Write-Log "  7. Iniciar servicio $ServiceName" "Yellow"
    Write-Log "  8. Verificar $HealthUrl" "Yellow"
    Write-Log "[WhatIf] Sin cambios ejecutados." "Yellow"
    exit 0
}

# --- Paso 1: Backup ---
if (-not $SkipBackup) {
    Write-Log "[1/7] Backup previo..." "Yellow"
    $backupScript = Join-Path (Split-Path -Parent $ScriptDir) "backup\Backup-CafeDeBarrio.ps1"
    $backupArgs   = @{ Server = $DbServer; Database = $DbName }
    if ($WindowsAuth) { $backupArgs.WindowsAuth = $true }
    else { $backupArgs.UserName = $DbUser; $backupArgs.Password = $DbPassword }
    & $backupScript @backupArgs
    if ($LASTEXITCODE -ne 0) { throw "Backup fallido. Deploy abortado." }
    Write-Log "  Backup completado." "Green"
} else {
    Write-Log "[1/7] Backup omitido." "Gray"
}

# --- Paso 2: Migraciones DB ---
Write-Log "[2/7] Aplicando migraciones de base de datos..." "Yellow"
$migrateScript = Join-Path (Split-Path -Parent $ScriptDir) "migration\apply-migration.ps1"
$migrateArgs   = @{ Server = $DbServer; Database = $DbName; SqlScriptPath = $sqlScript; SkipBackup = $true }
if ($WindowsAuth) { $migrateArgs.WindowsAuth = $true }
else { $migrateArgs.UserName = $DbUser; $migrateArgs.Password = $DbPassword }
& $migrateScript @migrateArgs
if ($LASTEXITCODE -ne 0) { throw "Migracion fallida. Deploy abortado. DB sin cambios de binarios." }
Write-Log "  Migraciones aplicadas." "Green"

# --- Paso 3: Parar servicio ---
Write-Log "[3/7] Deteniendo servicio $ServiceName..." "Yellow"
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($svc -and $svc.Status -eq "Running") {
    Stop-Service -Name $ServiceName -Force
    $svc.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(30))
    Write-Log "  Servicio detenido." "Green"
} else {
    Write-Log "  Servicio no encontrado o ya detenido." "Gray"
}

# --- Paso 4: Copiar API ---
Write-Log "[4/7] Copiando binarios de API..." "Yellow"
if (-not (Test-Path $ApiDestDir)) { New-Item -ItemType Directory -Path $ApiDestDir | Out-Null }
robocopy $apiSource $ApiDestDir /MIR /XF ".env" /NJH /NJS /NFL /NDL
if ($LASTEXITCODE -ge 8) { throw "Robocopy fallo con codigo $LASTEXITCODE" }
Write-Log "  Binarios copiados." "Green"

# --- Paso 5-6: Frontends ---
if (-not $SkipFrontend) {
    foreach ($pair in @(
        @{ src = $dashSource; dest = $DashDestDir; name = "Dashboard" },
        @{ src = $posSource;  dest = $PosDestDir;  name = "POS PWA"  }
    )) {
        if (Test-Path $pair.src) {
            Write-Log "[$($pair.name)] Copiando estaticos -> $($pair.dest)..." "Yellow"
            if (-not (Test-Path $pair.dest)) { New-Item -ItemType Directory -Path $pair.dest | Out-Null }
            robocopy $pair.src $pair.dest /MIR /NJH /NJS /NFL /NDL
            if ($LASTEXITCODE -ge 8) { throw "Robocopy fallo para $($pair.name)" }
            Write-Log "  $($pair.name) copiado." "Green"
        } else {
            Write-Log "  $($pair.name) no encontrado en release/ - omitido." "Gray"
        }
    }
} else {
    Write-Log "[5-6/7] Frontends omitidos (-SkipFrontend)." "Gray"
}

# --- Paso 7: Iniciar servicio ---
Write-Log "[7/7] Iniciando servicio $ServiceName..." "Yellow"
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($svc) {
    Start-Service -Name $ServiceName
    $svc.WaitForStatus("Running", [TimeSpan]::FromSeconds(30))
    Write-Log "  Servicio iniciado." "Green"

    $maxWait = 60
    $elapsed = 0
    while ($elapsed -lt $maxWait) {
        try {
            $resp = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 5
            if ($resp.StatusCode -eq 200) { break }
        } catch { }
        Start-Sleep -Seconds 3
        $elapsed += 3
    }
    if ($elapsed -ge $maxWait) {
        Write-Log "  ADVERTENCIA: health check no respondio en ${maxWait}s. Verificar manualmente: $HealthUrl" "Yellow"
    } else {
        Write-Log "  Health check OK: $HealthUrl" "Green"
    }
} else {
    Write-Log "  Servicio $ServiceName no registrado. Iniciar manualmente. Ver runbook seccion 4." "Yellow"
}

Write-Log "==========================================" "Green"
Write-Log "  Deploy v$Version completado." "Green"
Write-Log "  Log: $logFile" "Green"
Write-Log "==========================================" "Green"
