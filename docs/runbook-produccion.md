# Runbook de Producción — CafeDeBarrio POS

Versión mínima del sistema: 1.0.0  
Actualizado: 2026-06-09

---

## 1. Requisitos del servidor

| Componente | Mínimo | Notas |
|---|---|---|
| OS | Windows Server 2019 o Windows 10 Pro | 64-bit |
| .NET Runtime | 9.0 (ASP.NET Core Runtime) | [descargar](https://dotnet.microsoft.com/download/dotnet/9) |
| SQL Server | Express 2019+ | Edición Express OK para ≤50 usuarios |
| IIS | 10 + ASP.NET Core Hosting Bundle | Para frontends estáticos |
| NSSM | 2.24+ | Para registrar la API como Windows Service |
| Node.js | 20 LTS | Solo en máquina de build, no en producción |
| RAM | 4 GB mínimo | 2 GB para SQL Server + 1 GB API + SO |
| Disco | 20 GB | SO + SQL data + logs rotativos 30 días |

---

## 2. Estructura de directorios en producción

```
C:\Services\CafeBarrioAPI\     ← binarios de la API
    CafeBarrio.API.exe
    appsettings.json
    .env                        ← SECRETOS — no sobreescribir en updates
    logs\

C:\inetpub\wwwroot\
    cafebarrio-dashboard\      ← archivos estáticos del dashboard
    cafebarrio-pos\            ← archivos estáticos del POS PWA

C:\SQLBackups\                 ← backups de base de datos
    CafeDeBarrio_YYYYMMDD.bak
```

---

## 3. Primera instalación (servidor nuevo)

### 3.1 Preparar SQL Server

```sql
-- En SQL Server Management Studio como sysadmin:
CREATE DATABASE CafeDeBarrio;
GO
```

Luego crear el login de mínimo privilegio (no `db_owner`):

```powershell
# Editar STRONG_PASSWORD_HERE antes de ejecutar
sqlcmd -S "PROD-SERVER\SQLEXPRESS" -E -i scripts\sql\create-app-login.sql
```

El script crea el login `app_cafebarrio` con `db_datareader + db_datawriter + EXECUTE` (sin `db_owner`).

### 3.2 Aplicar migraciones

```powershell
# Desde la maquina de build (con el paquete release/):
.\scripts\migration\apply-migration.ps1 `
    -Server "PROD-SERVER\SQLEXPRESS" `
    -Database "CafeDeBarrio" `
    -UserName "app_cafebarrio" `
    -Password "STRONG_PASSWORD_HERE"
```

El script aplica la migración `InitialCreate_Baseline` (migración única — 29 previas consolidadas en Sprint V7).

**IMPORTANTE:** Ejecutar este script ANTES de iniciar la API por primera vez. La API en producción no aplica migraciones automáticamente (G-INF-005) y lanza excepción si hay migraciones pendientes al arrancar.

### 3.3 Crear el archivo `.env`

Crear `C:\Services\CafeBarrioAPI\.env` con los valores de producción:

```ini
# JWT — generar con: [Convert]::ToBase64String((1..64 | % { Get-Random -Max 256 }))
Jwt__Key=REEMPLAZAR_CON_CLAVE_64_BYTES_BASE64

# Admin inicial — cambiar el password después del primer login
Seed__AdminPassword=CambiarEsto2026!

# Base de datos de produccion
# IMPORTANTE: TrustServerCertificate=True está PROHIBIDO en producción — la API lanza excepción al arrancar si lo detecta.
# Configurar certificado TLS válido en SQL Server o usar Windows Auth (Integrated Security=True).
ConnectionStrings__DefaultConnection=Server=PROD-SERVER\SQLEXPRESS;Database=CafeDeBarrio;User Id=app_cafebarrio;Password=STRONG_PASSWORD_HERE;Encrypt=True;

# SUNAT (dejar deshabilitado hasta tener cliente con RUC)
Sunat__Enabled=false
Sunat__Ruc=
Sunat__OseToken=

# CORS — URL donde se sirve el dashboard
Cors__AllowedOrigin=https://dashboard.tucafe.com
```

**Este archivo NUNCA se sube a git y NUNCA se sobreescribe en actualizaciones.**

### 3.4 Registrar la API como Windows Service (NSSM)

```powershell
# Instalar NSSM y registrar el servicio:
nssm install CafeBarrioAPI "C:\Services\CafeBarrioAPI\CafeBarrio.API.exe"
nssm set CafeBarrioAPI AppDirectory "C:\Services\CafeBarrioAPI"
nssm set CafeBarrioAPI AppEnvironmentExtra "ASPNETCORE_ENVIRONMENT=Production"
nssm set CafeBarrioAPI AppStdout "C:\Services\CafeBarrioAPI\logs\stdout.log"
nssm set CafeBarrioAPI AppStderr "C:\Services\CafeBarrioAPI\logs\stderr.log"
nssm set CafeBarrioAPI Start SERVICE_AUTO_START
nssm start CafeBarrioAPI
```

### 3.5 Configurar IIS para frontends

```powershell
# Dashboard
New-WebSite -Name "CafeBarrioDashboard" `
    -PhysicalPath "C:\inetpub\wwwroot\cafebarrio-dashboard" `
    -Port 8080

# POS PWA
New-WebSite -Name "CafeBarrioPOS" `
    -PhysicalPath "C:\inetpub\wwwroot\cafebarrio-pos" `
    -Port 8081
```

Agregar `web.config` en cada carpeta de frontend para manejar rutas SPA:

```xml
<?xml version="1.0"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="SPA" stopProcessing="true">
          <match url=".*" />
          <conditions>
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### 3.6 Permisos de backup

SQL Server necesita acceso de escritura a `C:\SQLBackups\`:

```powershell
# El servicio SQL Server corre tipicamente como NT Service\MSSQLSERVER
icacls "C:\SQLBackups" /grant "NT Service\MSSQLSERVER:(OI)(CI)F"
```

---

## 4. Actualización (deploy de nueva versión)

```powershell
# Desde la maquina de build, con el paquete release/ de la nueva version:
.\scripts\deploy\Deploy-Production.ps1 `
    -Version "1.0.1" `
    -DbServer "PROD-SERVER\SQLEXPRESS" `
    -DbUser "cafebarrio_app" `
    -DbPassword "PasswordSeguro2026!"
```

El script ejecuta automáticamente: backup → migraciones → parar servicio → copiar binarios → iniciar → verificar health.

Para preview sin ejecutar: agregar `-WhatIf`.

---

## 5. Rollback

Si el deploy falla y el servicio no levanta:

```powershell
# 1. Parar el servicio
Stop-Service CafeBarrioAPI

# 2. Restaurar backup del dia (ajustar nombre de archivo)
$bak = "C:\SQLBackups\CafeDeBarrio_YYYYMMDD_HHMMSS.bak"
sqlcmd -S "PROD-SERVER\SQLEXPRESS" -U cafebarrio_app -P "..." -Q "
    RESTORE DATABASE CafeDeBarrio
    FROM DISK = N'$bak'
    WITH REPLACE, RECOVERY;"

# 3. Restaurar binarios anteriores (si se guardaron)
# Copiar de C:\Services\CafeBarrioAPI_backup_YYYYMMDD\ si existe

# 4. Reiniciar servicio
Start-Service CafeBarrioAPI
```

**Practica recomendada:** antes de cada deploy, copiar `C:\Services\CafeBarrioAPI\` a `C:\Services\CafeBarrioAPI_backup_YYYYMMDD\` (excluyendo `.env`).

---

## 6. Activación de SUNAT (cuando se tenga cliente con RUC)

Editar `C:\Services\CafeBarrioAPI\.env`:

```ini
Sunat__Enabled=true
Sunat__Ruc=20XXXXXXXXX
Sunat__RazonSocial=Cafe de Barrio S.A.C.
Sunat__NombreComercial=Cafe de Barrio
Sunat__DireccionFiscal=Jr. Principal 123, Lima
Sunat__OseToken=TOKEN_DE_NUBEFACT
Sunat__SerieBoletaBx=B001
Sunat__PorcentajeIgv=18.0
```

Reiniciar el servicio:

```powershell
Restart-Service CafeBarrioAPI
```

No se requiere rebuild ni redeploy — la activación es solo configuración.

---

## 7. Verificación post-despliegue

```powershell
# Health check de la API:
Invoke-WebRequest http://localhost:5000/health

# Verificar migraciones aplicadas:
sqlcmd -S "PROD-SERVER\SQLEXPRESS" -U app_cafebarrio -P "..." `
    -Q "SELECT COUNT(*) FROM CafeDeBarrio.dbo.__EFMigrationsHistory"
# Resultado esperado: 1 (InitialCreate_Baseline — 29 migraciones consolidadas en Sprint V7)

# Verificar logs de la API:
Get-Content "C:\Services\CafeBarrioAPI\logs\cafebarrio-*.log" -Tail 20
```

---

## 8. Backups manuales

```powershell
.\scripts\backup\Backup-CafeDeBarrio.ps1 `
    -Server "PROD-SERVER\SQLEXPRESS" `
    -Database "CafeDeBarrio" `
    -UserName "cafebarrio_app" `
    -Password "PasswordSeguro2026!" `
    -BackupDir "C:\SQLBackups"
```

Los backups se retienen por 7 ciclos (política definida en el script).

---

## 9. Resolución de problemas comunes

| Síntoma | Causa probable | Solución |
|---|---|---|
| API no inicia — `InvalidOperationException: Seed:AdminPassword no configurado` | `.env` no existe o no se cargó | Verificar que `.env` esté en `C:\Services\CafeBarrioAPI\` |
| `Login failed for user` | Credenciales incorrectas en `.env` | Revisar `ConnectionStrings__DefaultConnection` en `.env` |
| `There is already an object named '...'` en migraciones | API se inició antes de aplicar migraciones (`EnsureCreated` creó tablas sin historial) | Insertar registros manualmente en `__EFMigrationsHistory` para las migraciones ya aplicadas, luego re-ejecutar `apply-migration.ps1` |
| Health check retorna 503 | DB no disponible | Verificar conectividad a SQL Server y que el servicio esté corriendo |
| CORS error en dashboard | URL de producción no en `Cors__AllowedOrigin` | Actualizar `.env` y reiniciar servicio |
| Boleta no emitida, `SunatEstado=NoEmitida` | `Sunat__Enabled=false` o token inválido | Verificar configuración SUNAT en `.env` |
