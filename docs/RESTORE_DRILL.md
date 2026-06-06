# Restore Drill — Cafe de Barrio
Fecha de referencia: 2026-06-06
Tiempo estimado: 15-20 minutos
Ejecutar: trimestralmente o despues de cualquier incidente de datos.

---

## Objetivo
Verificar que un backup real puede restaurarse y el sistema vuelve a operar
normalmente. Si este drill falla, el backup no es util.

---

## Pre-requisitos
- Archivo de backup disponible: CafeDeBarrio_YYYYMMDD_HHmmss.bak
- Docker Desktop corriendo con contenedor sqlserver activo
- API detenida durante la restauracion

---

## Procedimiento

### Etapa 1 — Detener servicios
1. Detener la API (Ctrl+C en consola, o detener el servicio de Windows si esta registrado)
2. Cerrar todos los clientes POS conectados
3. Confirmar que no hay conexiones activas a la BD:
   docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Muis_CafeBarrio_2026!" -No -Q "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE database_id = DB_ID('CafeDeBarrio')"
   Resultado esperado: 0 conexiones (aparte de la propia consulta)

### Etapa 2 — Copiar el backup al contenedor
docker cp "C:\ruta\al\archivo.bak" sqlserver:/var/opt/mssql/backups/restore_drill.bak

### Etapa 3 — Restaurar la base de datos
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd ^
  -S localhost -U sa -P "Muis_CafeBarrio_2026!" -No ^
  -Q "ALTER DATABASE [CafeDeBarrio] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [CafeDeBarrio] FROM DISK = N'/var/opt/mssql/backups/restore_drill.bak' WITH REPLACE, STATS = 10; ALTER DATABASE [CafeDeBarrio] SET MULTI_USER;"

Esperar a que termine. Resultado esperado: mensajes STATS y "RESTORE DATABASE successfully processed".

### Etapa 4 — Verificar integridad
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Muis_CafeBarrio_2026!" -No -Q "DBCC CHECKDB([CafeDeBarrio]) WITH NO_INFOMSGS"

Resultado esperado: sin mensajes de error.

### Etapa 5 — Reiniciar y verificar aplicacion
1. Iniciar la API
2. GET https://localhost:7240/api/categorias → 200 + categorias
3. POST /api/auth/login con admin@cafedebarrio.com / Admin2026! → 200 + token
4. GET https://localhost:7240/api/transacciones → verificar que las transacciones del backup esten presentes
5. Abrir el POS → realizar una venta de prueba → confirmar que se registra

### Etapa 6 — Documentar resultado

| Campo | Valor |
|-------|-------|
| Fecha del drill | |
| Archivo .bak usado | |
| Fecha del backup | |
| Transacciones recuperadas | |
| Paso que fallo (si aplica) | |
| Tiempo total | |
| Ejecutado por | |

---

## Script rapido de verificacion post-restore

Guardar como scripts/backup/Verify-Restore.ps1 y ejecutar despues de la restauracion:

param(
    [string]$SqlServer   = "localhost,1433",
    [string]$SqlUser     = "sa",
    [string]$SqlPassword = "Muis_CafeBarrio_2026!"
)
$checks = @(
    "SELECT COUNT(*) AS Sedes          FROM Sede",
    "SELECT COUNT(*) AS Transacciones  FROM Transaccion",
    "SELECT COUNT(*) AS Productos       FROM Producto",
    "SELECT COUNT(*) AS Usuarios        FROM Usuario"
)
foreach ($q in $checks) {
    docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd `
        -S $SqlServer -U $SqlUser -P $SqlPassword -No -Q $q
}
