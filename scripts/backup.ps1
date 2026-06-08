param(
    [string]$Server     = "localhost,1433",
    [string]$Database   = "CafeDeBarrio",
    [string]$BackupDir  = ".\backups",
    [int]   $RetainCount = 7
)

$timestamp  = Get-Date -Format "yyyy-MM-dd_HHmmss"
$backupFile = Join-Path $BackupDir "$Database-$timestamp.bak"
$saPassword = $env:MSSQL_SA_PASSWORD
if (-not $saPassword) { Write-Error "[BACKUP] MSSQL_SA_PASSWORD no configurado."; exit 1 }

if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
    Write-Output "[BACKUP] $timestamp — Directorio creado: $BackupDir"
}

Write-Output "[BACKUP] $timestamp — Iniciando backup de '$Database' en '$Server'..."

$query = "BACKUP DATABASE [$Database] TO DISK = N'$backupFile' WITH INIT, COMPRESSION, STATS = 10"
sqlcmd -S $Server -U sa -P $saPassword -Q $query -C -No
if ($LASTEXITCODE -ne 0) {
    Write-Error "[BACKUP] $timestamp — FALLO. Exit code: $LASTEXITCODE"
    exit 1
}

Write-Output "[BACKUP] $timestamp — Backup completado: $backupFile"

$allBackups = Get-ChildItem -Path $BackupDir -Filter "$Database-*.bak" |
              Sort-Object LastWriteTime -Descending
if ($allBackups.Count -gt $RetainCount) {
    $toDelete = $allBackups | Select-Object -Skip $RetainCount
    foreach ($file in $toDelete) {
        Remove-Item $file.FullName -Force
        Write-Output "[BACKUP] $timestamp — Rotado (eliminado): $($file.Name)"
    }
}

Write-Output "[BACKUP] $timestamp — Proceso finalizado. Backups retenidos: $([Math]::Min($allBackups.Count, $RetainCount))"
