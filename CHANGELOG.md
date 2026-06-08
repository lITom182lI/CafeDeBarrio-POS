# Changelog

## [1.0.0] — 2026-06-07

### Funcionalidades

- Sistema de punto de venta offline-first (PWA + IndexedDB + sincronización automática)
- Panel de administración web con reportes (ventas, métodos de pago, franja horaria, stock)
- Cliente de escritorio WinForms para Windows
- Autenticación JWT con Argon2id y rate limiting
- CRUD de productos, operadores, turnos y movimientos de caja
- Audit trail automático (CreatedBy/UpdatedBy) en todas las entidades principales
- Soporte de pago dual (efectivo + segundo método) y comprobante nominado
- Seed automático de datos de referencia en instalación nueva

### Infraestructura

- Clean Architecture (.NET 9 + EF Core 9 + SQL Server)
- CI/CD con GitHub Actions (build + unit tests + integration tests + vitest)
- Configuración de producción por variables de entorno
- 9 migraciones EF Core + script de backup PowerShell
