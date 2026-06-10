<#
.SYNOPSIS
    Aplica las migraciones de base de datos de CafeDeBarrio a produccion.
.DESCRIPTION
    1. Crea un backup completo antes de migrar (omitible con -SkipBackup).
    2. Aplica migrate-production.sql de forma idempotente via sqlcmd.
    3. Verifica que todas las migraciones quedaron registradas en __EFMigrationsHistory.
    4. Escribe un log timestamped en scripts/migration/logs/.
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$Server,

    [string]$Database      = "CafeDeBarrio",
    [string]$UserName      = "",
    [string]$Password      = "",
    [switch]$WindowsAuth,

    [string]$BackupDir     = "$env:USERPROFILE\CafeDeBarrio\backups",
    [string]$SqlScriptPath = "",
    [switch]$SkipBackup,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }
if (-not $SqlScriptPath) { $SqlScriptPath = Join-Path $ScriptDir "migrate-production.sql" }

$logDir  = Join-Path $ScriptDir "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
$logFile = "$logDir\migration_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Log {
    param([string]$Msg, [string]$Color = "White")
    $line = "[$(Get-Date -Format 'HH:mm:ss')] $Msg"
    Write-Host $line -ForegroundColor $Color
    Add-Content -Path $logFile -Value $line
}

function Get-AuthArgs {
    if ($WindowsAuth) { return @("-E") }
    return @("-U", $UserName, "-P", $Password)
}

function Invoke-Sqlcmd-Query {
    param([string]$Query)
    $authArgs = Get-AuthArgs
    $result = & sqlcmd -S $Server -d $Database @authArgs -Q $Query -h -1 -W -C 2>&1
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd error: $result" }
    return $result
}

Write-Log "===============================================" "Cyan"
Write-Log "  CafeDeBarrio -- Migration to Production" "Cyan"
Write-Log "  Server  : $Server" "Cyan"
Write-Log "  Database: $Database" "Cyan"
Write-Log "  WhatIf  : $WhatIf" "Cyan"
Write-Log "===============================================" "Cyan"

if (-not (Test-Path $SqlScriptPath)) {
    throw "Script SQL no encontrado: $SqlScriptPath"
}

if ($WhatIf) {
    Write-Log "[WhatIf] Se aplicaria: $SqlScriptPath" "Yellow"
    Write-Log "[WhatIf] Backup previo: $(-not $SkipBackup)" "Yellow"
    Write-Log "[WhatIf] Sin cambios ejecutados." "Yellow"
    exit 0
}

if (-not $SkipBackup) {
    Write-Log "[1/3] Ejecutando backup previo..." "Yellow"
    $backupScript = Join-Path (Split-Path -Parent $ScriptDir) "backup\Backup-CafeDeBarrio.ps1"
    if (Test-Path $backupScript) {
        $backupArgs = @{ Server = $Server; Database = $Database; BackupDir = $BackupDir }
        if (-not $WindowsAuth) {
            $backupArgs.UserName = $UserName
            $backupArgs.Password = $Password
        } else {
            $backupArgs.WindowsAuth = $true
        }
        & $backupScript @backupArgs
        if ($LASTEXITCODE -ne 0) { throw "Backup fallido. Migracion abortada." }
        Write-Log "  Backup completado." "Green"
    } else {
        Write-Log "  Backup script no encontrado -- omitiendo (riesgo aceptado)." "Yellow"
    }
} else {
    Write-Log "[1/3] Backup omitido (-SkipBackup)." "Gray"
}

Write-Log "[2/3] Aplicando migraciones idempotentes..." "Yellow"

$authArgs = Get-AuthArgs
& sqlcmd -S $Server -d $Database @authArgs -i $SqlScriptPath -b -C 2>&1 | Tee-Object -FilePath $logFile -Append

if ($LASTEXITCODE -ne 0) {
    Write-Log "  ERROR aplicando migraciones. Ver log: $logFile" "Red"
    exit 1
}
Write-Log "  Script aplicado." "Green"

Write-Log "[3/3] Verificando historial de migraciones..." "Yellow"

$historial = Invoke-Sqlcmd-Query "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId"
$count     = ($historial | Where-Object { $_ -match '^\d{14}_' }).Count

Write-Log "  Migraciones aplicadas: $count" "Cyan"
$historial | Where-Object { $_ -match '^\d{14}_' } | ForEach-Object {
    Write-Log "    * $($_.Trim())" "Green"
}

$expected = 16
if ($count -lt $expected) {
    Write-Log "  ADVERTENCIA: Se esperaban $expected migraciones, se encontraron $count." "Yellow"
} else {
    Write-Log "  Verificacion correcta: $count/$expected migraciones." "Green"
}

Write-Log "===============================================" "Cyan"
Write-Log "  Migracion completada. Log: $logFile" "Green"
Write-Log "===============================================" "Cyan"
