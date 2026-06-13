-- Script para crear el login dedicado de la aplicación.
-- Ejecutar en la instancia SQL Server de PRODUCCIÓN con una cuenta sysadmin.
-- Reemplazar 'STRONG_PASSWORD_HERE' con la contraseña real antes de ejecutar.
-- Nunca commitear este archivo con credenciales reales.

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'app_cafebarrio')
BEGIN
    CREATE LOGIN [app_cafebarrio]
        WITH PASSWORD = N'STRONG_PASSWORD_HERE',
             CHECK_POLICY = ON,
             CHECK_EXPIRATION = ON;
END
GO

USE CafeDeBarrio;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'app_cafebarrio')
BEGIN
    CREATE USER [app_cafebarrio] FOR LOGIN [app_cafebarrio];
END
GO

ALTER ROLE db_datareader ADD MEMBER [app_cafebarrio];
ALTER ROLE db_datawriter ADD MEMBER [app_cafebarrio];
GRANT EXECUTE ON SCHEMA::dbo TO [app_cafebarrio];
GO
