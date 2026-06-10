param(
    [Parameter(Mandatory=$true)] [string]$Version,
    [string]$OutputDir = ".\release",
    [switch]$SkipSign
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "    MUIS Build Pipeline -- Cafe de Barrio   " -ForegroundColor Cyan
Write-Host "    Version: $Version" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# ── PASO 0: Validaciones ────────────────────────────────────────────────
Write-Host "[0/6] Validando prerrequisitos..." -ForegroundColor Yellow
dotnet list ".\src" package --vulnerable --include-transitive 2>&1 |
    Where-Object { $_ -match "vulnerable" } |
    ForEach-Object { Write-Warning $_ }

if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path "$OutputDir\api" | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\pos" | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\dashboard" | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\pos-pwa"  | Out-Null
Write-Host "  ✅ Prerrequisitos OK" -ForegroundColor Green

# ── PASO 1: Unit Tests ──────────────────────────────────────────────────
Write-Host "[1/6] Ejecutando Unit Tests..." -ForegroundColor Yellow
dotnet test ".\tests\CafeBarrio.Tests.Unit" `
    --filter "Category!=Integration" --no-build --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0) { throw "Unit tests fallidos. Build abortado." }
Write-Host "  ✅ Unit tests pasados" -ForegroundColor Green

# ── PASO 2: Build Frontends ─────────────────────────────────────────────
Write-Host "[2/6] Compilando frontends React..." -ForegroundColor Yellow

# Dashboard
New-Item -ItemType Directory -Path "$OutputDir\dashboard" -Force | Out-Null
Push-Location ".\dashboard"
npm ci --silent
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Build de dashboard fallido." }
Copy-Item -Path ".\dist\*" -Destination "..\$OutputDir\dashboard" -Recurse
Pop-Location
Write-Host "  ✅ Dashboard compilado -> $OutputDir/dashboard/" -ForegroundColor Green

# POS PWA
New-Item -ItemType Directory -Path "$OutputDir\pos-pwa" -Force | Out-Null
Push-Location ".\pos-pwa"
npm ci --silent
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Build de POS PWA fallido." }
Copy-Item -Path ".\dist\*" -Destination "..\$OutputDir\pos-pwa" -Recurse
Pop-Location
Write-Host "  ✅ POS PWA compilado -> $OutputDir/pos-pwa/" -ForegroundColor Green

# ── PASO 3: Publish API ─────────────────────────────────────────────────
Write-Host "[3/6] Publicando API backend..." -ForegroundColor Yellow
dotnet publish ".\src\CafeBarrio.API\CafeBarrio.API.csproj" `
    --configuration Release `
    --output "$OutputDir\api" `
    -p:Version=$Version `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "Publicación de API fallida." }
Write-Host "  ✅ API publicada → $OutputDir/api/" -ForegroundColor Green

# ── PASO 4: Publish POS WinForms ────────────────────────────────────────
Write-Host "[4/6] Publicando POS WinForms (self-contained win-x64)..." -ForegroundColor Yellow
dotnet publish ".\pos-client\CafeBarrio.POS.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output "$OutputDir\pos" `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:Version=$Version `
    --nologo
if ($LASTEXITCODE -ne 0) { throw "Publicación del POS fallida." }

$appConfigExample = ".\config\App.config.example"
$appConfigDest    = "$OutputDir\pos\App.config"
if ((Test-Path $appConfigExample) -and (-not (Test-Path $appConfigDest))) {
    Copy-Item $appConfigExample $appConfigDest
}
Write-Host "  ✅ POS publicado → $OutputDir/pos/" -ForegroundColor Green

# ── PASO 5: Empaquetado ─────────────────────────────────────────────────
Write-Host "[5/6] Empaquetando..." -ForegroundColor Yellow
$innoCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $innoCompiler) {
    & $innoCompiler /DAppVersion=$Version ".\scripts\installer\CafeDeBarrio.iss"
    if ($LASTEXITCODE -ne 0) { throw "Generación del instalador fallida." }
    Write-Host "  ✅ Instalador .exe generado en release/" -ForegroundColor Green
} else {
    Compress-Archive -Path "$OutputDir\pos\*" `
        -DestinationPath "$OutputDir\CafeDeBarrio-POS-v$Version.zip" -Force
    Write-Host "  ✅ ZIP portable generado (Inno Setup no instalado)." -ForegroundColor Green
}

# ── PASO 6: Generar script SQL de migración ─────────────────────────────────
Write-Host "[6/6] Generando script de migración producción..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path ".\scripts\migration" | Out-Null
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef migrations script --idempotent `
    --project ".\src\CafeBarrio.Infrastructure" `
    --startup-project ".\src\CafeBarrio.API" `
    --output ".\scripts\migration\migrate-production.sql" `
    --no-build
if ($LASTEXITCODE -ne 0) { Write-Warning "No se pudo regenerar el script SQL de migración." }
else {
    $sqlSize = [Math]::Round((Get-Item ".\scripts\migration\migrate-production.sql").Length / 1KB, 1)
    Write-Host "  ✅ migrate-production.sql generado ($sqlSize KB)" -ForegroundColor Green
    Copy-Item ".\scripts\migration\migrate-production.sql" "$OutputDir\migrate-production.sql"
}

# ── RESUMEN ─────────────────────────────────────────────────────────────
Write-Host ""
$gitCommit = (git rev-parse --short HEAD 2>$null)
$archivos  = Get-ChildItem $OutputDir -File |
             ForEach-Object { "  - $($_.Name) ($([Math]::Round($_.Length/1MB,2)) MB)" }

@"
════════════════════════════════════════
  MUIS Release Report — Café de Barrio
  Versión   : $Version
  Fecha     : $(Get-Date -Format "yyyy-MM-dd HH:mm")
  Git commit: $gitCommit
  Archivos  :
$($archivos -join "`n")
════════════════════════════════════════
"@ | Tee-Object -FilePath "$OutputDir\release-report.txt"

Write-Host "==============================================" -ForegroundColor Green
Write-Host "    [OK] Build v$Version completado         " -ForegroundColor Green
Write-Host "==============================================" -ForegroundColor Green
