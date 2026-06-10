IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [CategoriaCafe] (
        [categoria_id] int NOT NULL IDENTITY,
        [codigo] nvarchar(10) NOT NULL,
        [nombre] nvarchar(100) NOT NULL,
        [descripcion] nvarchar(300) NULL,
        [activa] bit NOT NULL,
        CONSTRAINT [PK_CategoriaCafe] PRIMARY KEY ([categoria_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [MetodoPago] (
        [metodo_pago_id] int NOT NULL IDENTITY,
        [nombre] nvarchar(100) NOT NULL,
        [activo] bit NOT NULL,
        CONSTRAINT [PK_MetodoPago] PRIMARY KEY ([metodo_pago_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [OpcionEnvio] (
        [opcion_envio_id] int NOT NULL IDENTITY,
        [nombre] nvarchar(100) NOT NULL,
        [descripcion] nvarchar(300) NULL,
        [tarifa] decimal(18,2) NOT NULL,
        [activa] bit NOT NULL,
        CONSTRAINT [PK_OpcionEnvio] PRIMARY KEY ([opcion_envio_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [Sede] (
        [sede_id] int NOT NULL IDENTITY,
        [nombre] nvarchar(100) NOT NULL,
        [direccion] nvarchar(255) NOT NULL,
        [distrito] nvarchar(100) NOT NULL,
        [ciudad] nvarchar(100) NOT NULL,
        [telefono] nvarchar(20) NULL,
        [es_principal] bit NOT NULL,
        [activa] bit NOT NULL,
        [fecha_apertura] date NULL,
        CONSTRAINT [PK_Sede] PRIMARY KEY ([sede_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [TipoCliente] (
        [tipo_cliente_id] int NOT NULL IDENTITY,
        [nombre] nvarchar(50) NOT NULL,
        [descripcion] nvarchar(200) NULL,
        CONSTRAINT [PK_TipoCliente] PRIMARY KEY ([tipo_cliente_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [Transporte] (
        [transporte_id] int NOT NULL IDENTITY,
        [placa] nvarchar(20) NOT NULL,
        [descripcion] nvarchar(200) NULL,
        [capacidad_kg] decimal(10,2) NULL,
        [disponible] bit NOT NULL,
        [observaciones] nvarchar(max) NULL,
        CONSTRAINT [PK_Transporte] PRIMARY KEY ([transporte_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [UbicacionPreferencia] (
        [ubicacion_id] int NOT NULL IDENTITY,
        [nombre] nvarchar(100) NOT NULL,
        [descripcion] nvarchar(200) NULL,
        CONSTRAINT [PK_UbicacionPreferencia] PRIMARY KEY ([ubicacion_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [Producto] (
        [producto_id] int NOT NULL IDENTITY,
        [categoria_id] int NOT NULL,
        [nombre] nvarchar(200) NOT NULL,
        [descripcion] nvarchar(max) NULL,
        [costo] decimal(18,2) NOT NULL,
        [precio] decimal(18,2) NOT NULL,
        [cantidad_disponible] int NOT NULL,
        [cantidad_por_unidad] nvarchar(50) NULL,
        [imagen_url] nvarchar(500) NULL,
        [es_mayorista] bit NOT NULL,
        [activo] bit NOT NULL,
        [fecha_creacion] datetime2 NOT NULL,
        [fecha_actualizacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Producto] PRIMARY KEY ([producto_id]),
        CONSTRAINT [FK_Producto_CategoriaCafe_categoria_id] FOREIGN KEY ([categoria_id]) REFERENCES [CategoriaCafe] ([categoria_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [Cliente] (
        [cliente_id] int NOT NULL IDENTITY,
        [tipo_cliente_id] int NOT NULL,
        [nombre] nvarchar(100) NOT NULL,
        [apellido] nvarchar(100) NOT NULL,
        [email] nvarchar(255) NOT NULL,
        [codigo_cliente] nvarchar(20) NULL,
        [tipo_documento] nvarchar(20) NULL,
        [numero_documento] nvarchar(50) NULL,
        [telefono] nvarchar(20) NULL,
        [direccion] nvarchar(300) NULL,
        [distrito] nvarchar(100) NULL,
        [ciudad] nvarchar(100) NULL,
        [ubicacion_id] int NULL,
        [categoria_pref_id] int NULL,
        [fecha_registro] date NOT NULL,
        [activo] bit NOT NULL,
        [observaciones] nvarchar(max) NULL,
        CONSTRAINT [PK_Cliente] PRIMARY KEY ([cliente_id]),
        CONSTRAINT [FK_Cliente_CategoriaCafe_categoria_pref_id] FOREIGN KEY ([categoria_pref_id]) REFERENCES [CategoriaCafe] ([categoria_id]),
        CONSTRAINT [FK_Cliente_TipoCliente_tipo_cliente_id] FOREIGN KEY ([tipo_cliente_id]) REFERENCES [TipoCliente] ([tipo_cliente_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Cliente_UbicacionPreferencia_ubicacion_id] FOREIGN KEY ([ubicacion_id]) REFERENCES [UbicacionPreferencia] ([ubicacion_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [Transaccion] (
        [transaccion_id] int NOT NULL IDENTITY,
        [cliente_id] int NOT NULL,
        [sede_id] int NOT NULL,
        [metodo_pago_id] int NOT NULL,
        [opcion_envio_id] int NULL,
        [es_mayorista] bit NOT NULL,
        [canal] nvarchar(20) NOT NULL,
        [fecha] datetime2 NOT NULL,
        [subtotal] decimal(18,2) NOT NULL,
        [impuesto] decimal(18,2) NOT NULL,
        [costo_envio] decimal(18,2) NOT NULL,
        [total] decimal(18,2) NOT NULL,
        [notas] nvarchar(max) NULL,
        CONSTRAINT [PK_Transaccion] PRIMARY KEY ([transaccion_id]),
        CONSTRAINT [FK_Transaccion_Cliente_cliente_id] FOREIGN KEY ([cliente_id]) REFERENCES [Cliente] ([cliente_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Transaccion_MetodoPago_metodo_pago_id] FOREIGN KEY ([metodo_pago_id]) REFERENCES [MetodoPago] ([metodo_pago_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Transaccion_OpcionEnvio_opcion_envio_id] FOREIGN KEY ([opcion_envio_id]) REFERENCES [OpcionEnvio] ([opcion_envio_id]),
        CONSTRAINT [FK_Transaccion_Sede_sede_id] FOREIGN KEY ([sede_id]) REFERENCES [Sede] ([sede_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [DetalleTransaccion] (
        [detalle_id] int NOT NULL IDENTITY,
        [transaccion_id] int NOT NULL,
        [producto_id] int NOT NULL,
        [cantidad] int NOT NULL,
        [precio_unitario] decimal(18,2) NOT NULL,
        [subtotal_linea] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetalleTransaccion] PRIMARY KEY ([detalle_id]),
        CONSTRAINT [FK_DetalleTransaccion_Producto_producto_id] FOREIGN KEY ([producto_id]) REFERENCES [Producto] ([producto_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DetalleTransaccion_Transaccion_transaccion_id] FOREIGN KEY ([transaccion_id]) REFERENCES [Transaccion] ([transaccion_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE TABLE [TransaccionMayorista] (
        [tm_id] int NOT NULL IDENTITY,
        [transaccion_id] int NOT NULL,
        [descuento_porcentaje] decimal(5,2) NOT NULL,
        [transporte_id] int NULL,
        [instrucciones_entrega] nvarchar(max) NULL,
        [fecha_entrega_estimada] date NULL,
        CONSTRAINT [PK_TransaccionMayorista] PRIMARY KEY ([tm_id]),
        CONSTRAINT [FK_TransaccionMayorista_Transaccion_transaccion_id] FOREIGN KEY ([transaccion_id]) REFERENCES [Transaccion] ([transaccion_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TransaccionMayorista_Transporte_transporte_id] FOREIGN KEY ([transporte_id]) REFERENCES [Transporte] ([transporte_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cliente_categoria_pref_id] ON [Cliente] ([categoria_pref_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cliente_tipo_cliente_id] ON [Cliente] ([tipo_cliente_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cliente_ubicacion_id] ON [Cliente] ([ubicacion_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetalleTransaccion_producto_id] ON [DetalleTransaccion] ([producto_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DetalleTransaccion_transaccion_id] ON [DetalleTransaccion] ([transaccion_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Producto_categoria_id] ON [Producto] ([categoria_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transaccion_cliente_id] ON [Transaccion] ([cliente_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transaccion_metodo_pago_id] ON [Transaccion] ([metodo_pago_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transaccion_opcion_envio_id] ON [Transaccion] ([opcion_envio_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transaccion_sede_id] ON [Transaccion] ([sede_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TransaccionMayorista_transaccion_id] ON [TransaccionMayorista] ([transaccion_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TransaccionMayorista_transporte_id] ON [TransaccionMayorista] ([transporte_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605165450_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260605165450_InitialCreate', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [operador_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [recargo_propina] decimal(10,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [turno_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Producto]') AND [c].[name] = N'cantidad_disponible');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Producto] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Producto] ALTER COLUMN [cantidad_disponible] decimal(10,3) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Producto] ADD [seguimiento_inventario] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Producto] ADD [stock_minimo] decimal(10,3) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Producto] ADD [unidad_medida] nvarchar(20) NOT NULL DEFAULT N'unidad';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE TABLE [ConfiguracionNegocio] (
        [ConfiguracionNegocioId] int NOT NULL IDENTITY,
        [SedeId] int NOT NULL,
        [TasaIGV] decimal(5,4) NOT NULL,
        [TasaIPM] decimal(5,4) NOT NULL,
        [FechaVigencia] datetime2 NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_ConfiguracionNegocio] PRIMARY KEY ([ConfiguracionNegocioId]),
        CONSTRAINT [FK_ConfiguracionNegocio_Sede_SedeId] FOREIGN KEY ([SedeId]) REFERENCES [Sede] ([sede_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE TABLE [Operador] (
        [OperadorId] int NOT NULL IDENTITY,
        [SedeId] int NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [PinHash] nvarchar(256) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Operador] PRIMARY KEY ([OperadorId]),
        CONSTRAINT [FK_Operador_Sede_SedeId] FOREIGN KEY ([SedeId]) REFERENCES [Sede] ([sede_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE TABLE [Anulacion] (
        [AnulacionId] int NOT NULL IDENTITY,
        [TransaccionId] int NOT NULL,
        [TipoAnulacion] nvarchar(20) NOT NULL,
        [Motivo] nvarchar(200) NOT NULL,
        [MontoOriginal] decimal(10,2) NOT NULL,
        [MontoDevuelto] decimal(10,2) NOT NULL,
        [MetodoDevolucion] nvarchar(50) NULL,
        [OperadorSolicitanteId] int NOT NULL,
        [AutorizadorId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [ImpactoInventario] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Anulacion] PRIMARY KEY ([AnulacionId]),
        CONSTRAINT [FK_Anulacion_Operador_AutorizadorId] FOREIGN KEY ([AutorizadorId]) REFERENCES [Operador] ([OperadorId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Anulacion_Operador_OperadorSolicitanteId] FOREIGN KEY ([OperadorSolicitanteId]) REFERENCES [Operador] ([OperadorId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Anulacion_Transaccion_TransaccionId] FOREIGN KEY ([TransaccionId]) REFERENCES [Transaccion] ([transaccion_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE TABLE [Turno] (
        [TurnoId] int NOT NULL IDENTITY,
        [SedeId] int NOT NULL,
        [OperadorId] int NOT NULL,
        [FechaApertura] datetime2 NOT NULL,
        [FechaCierre] datetime2 NULL,
        [MontoApertura] decimal(10,2) NOT NULL,
        [MontoEfectivoCierto] decimal(10,2) NULL,
        [TotalEfectivoSistema] decimal(10,2) NULL,
        [Estado] nvarchar(20) NOT NULL DEFAULT N'Abierto',
        [Observaciones] nvarchar(500) NULL,
        CONSTRAINT [PK_Turno] PRIMARY KEY ([TurnoId]),
        CONSTRAINT [FK_Turno_Operador_OperadorId] FOREIGN KEY ([OperadorId]) REFERENCES [Operador] ([OperadorId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Turno_Sede_SedeId] FOREIGN KEY ([SedeId]) REFERENCES [Sede] ([sede_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE TABLE [MovimientoCaja] (
        [MovimientoCajaId] int NOT NULL IDENTITY,
        [TurnoId] int NOT NULL,
        [TipoMovimiento] nvarchar(20) NOT NULL,
        [Motivo] nvarchar(200) NOT NULL,
        [Monto] decimal(10,2) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_MovimientoCaja] PRIMARY KEY ([MovimientoCajaId]),
        CONSTRAINT [FK_MovimientoCaja_Turno_TurnoId] FOREIGN KEY ([TurnoId]) REFERENCES [Turno] ([TurnoId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Transaccion_fecha] ON [Transaccion] ([fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Transaccion_operador_id] ON [Transaccion] ([operador_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Transaccion_turno_id] ON [Transaccion] ([turno_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Anulacion_AutorizadorId] ON [Anulacion] ([AutorizadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Anulacion_OperadorSolicitanteId] ON [Anulacion] ([OperadorSolicitanteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Anulacion_TransaccionId] ON [Anulacion] ([TransaccionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_ConfiguracionNegocio_SedeId_Activo] ON [ConfiguracionNegocio] ([SedeId], [Activo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_MovimientoCaja_TurnoId] ON [MovimientoCaja] ([TurnoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Operador_SedeId] ON [Operador] ([SedeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Turno_FechaApertura] ON [Turno] ([FechaApertura]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Turno_OperadorId] ON [Turno] ([OperadorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    CREATE INDEX [IX_Turno_SedeId] ON [Turno] ([SedeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Transaccion] ADD CONSTRAINT [FK_Transaccion_Operador_operador_id] FOREIGN KEY ([operador_id]) REFERENCES [Operador] ([OperadorId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    ALTER TABLE [Transaccion] ADD CONSTRAINT [FK_Transaccion_Turno_turno_id] FOREIGN KEY ([turno_id]) REFERENCES [Turno] ([TurnoId]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260605194524_Fase2_5_DomainFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260605194524_Fase2_5_DomainFix', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606001531_S4_IgvRegimenGeneral18Pct'
)
BEGIN
    EXEC(N'UPDATE [ConfiguracionNegocio] SET [TasaIGV] = 0.16, [TasaIPM] = 0.02, [FechaVigencia] = ''2026-06-05T00:00:00.0000000''
    WHERE [ConfiguracionNegocioId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606001531_S4_IgvRegimenGeneral18Pct'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606001531_S4_IgvRegimenGeneral18Pct', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606002310_S2_AuditTrail'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [created_at] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606002310_S2_AuditTrail'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [updated_at] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606002310_S2_AuditTrail'
)
BEGIN
    ALTER TABLE [Producto] ADD [created_at] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606002310_S2_AuditTrail'
)
BEGIN
    ALTER TABLE [Producto] ADD [updated_at] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606002310_S2_AuditTrail'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606002310_S2_AuditTrail', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606013023_V2_Auth_Usuario'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [UsuarioId] int NOT NULL IDENTITY,
        [Email] nvarchar(100) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [Rol] nvarchar(20) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([UsuarioId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606013023_V2_Auth_Usuario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606013023_V2_Auth_Usuario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606013023_V2_Auth_Usuario', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606210026_RemoveConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [ConfiguracionNegocio]
    WHERE [ConfiguracionNegocioId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260606210026_RemoveConfigSeed'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260606210026_RemoveConfigSeed', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    ALTER TABLE [Transaccion] DROP CONSTRAINT [FK_Transaccion_Cliente_cliente_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transaccion]') AND [c].[name] = N'cliente_id');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Transaccion] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Transaccion] ALTER COLUMN [cliente_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transaccion]') AND [c].[name] = N'canal');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Transaccion] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Transaccion] ADD DEFAULT N'POS' FOR [canal];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [numero_documento] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [razon_social] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [tipo_documento] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    ALTER TABLE [Transaccion] ADD CONSTRAINT [FK_Transaccion_Cliente_cliente_id] FOREIGN KEY ([cliente_id]) REFERENCES [Cliente] ([cliente_id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607065929_AddBoletaColumnsAndOptionalCliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260607065929_AddBoletaColumnsAndOptionalCliente', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607075854_AddMetodoPagoSecundario'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [metodo_pago_secundario_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607075854_AddMetodoPagoSecundario'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [monto_metodo_primario] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607075854_AddMetodoPagoSecundario'
)
BEGIN
    CREATE INDEX [IX_Transaccion_metodo_pago_secundario_id] ON [Transaccion] ([metodo_pago_secundario_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607075854_AddMetodoPagoSecundario'
)
BEGIN
    ALTER TABLE [Transaccion] ADD CONSTRAINT [FK_Transaccion_MetodoPago_metodo_pago_secundario_id] FOREIGN KEY ([metodo_pago_secundario_id]) REFERENCES [MetodoPago] ([metodo_pago_id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607075854_AddMetodoPagoSecundario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260607075854_AddMetodoPagoSecundario', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607224159_AddPerformanceIndexes'
)
BEGIN
    DROP INDEX [IX_Transaccion_sede_id] ON [Transaccion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607224159_AddPerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Transaccion_SedeId_Fecha] ON [Transaccion] ([sede_id], [fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607224159_AddPerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_Producto_Activo] ON [Producto] ([activo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260607224159_AddPerformanceIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260607224159_AddPerformanceIndexes', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Turno] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Turno] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Turno] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Turno] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Producto] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Producto] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Operador] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Operador] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Operador] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Operador] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [MovimientoCaja] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [MovimientoCaja] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [MovimientoCaja] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [MovimientoCaja] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Anulacion] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Anulacion] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Anulacion] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    ALTER TABLE [Anulacion] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260608020036_S3_AuditWithCreatedBy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260608020036_S3_AuditWithCreatedBy', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Turno].[IX_Turno_OperadorId]', N'IX_Turnos_OperadorId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Turno].[IX_Turno_FechaApertura]', N'IX_Turnos_FechaApertura', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Transaccion].[IX_Transaccion_turno_id]', N'IX_Transacciones_TurnoId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Transaccion].[IX_Transaccion_operador_id]', N'IX_Transacciones_OperadorId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Transaccion].[IX_Transaccion_fecha]', N'IX_Transacciones_Fecha', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Producto].[IX_Producto_categoria_id]', N'IX_Productos_CategoriaId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    EXEC sp_rename N'[Producto].[IX_Producto_Activo]', N'IX_Productos_Activo', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    CREATE INDEX [IX_Transacciones_Fecha_OperadorId] ON [Transaccion] ([fecha], [operador_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    CREATE INDEX [IX_Operadores_Activo] ON [Operador] ([Activo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609164751_AddPerformanceIndexes_Ref07'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609164751_AddPerformanceIndexes_Ref07', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194205_S5_DetalleTransaccionAudit'
)
BEGIN
    ALTER TABLE [DetalleTransaccion] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194205_S5_DetalleTransaccionAudit'
)
BEGIN
    ALTER TABLE [DetalleTransaccion] ADD [CreatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194205_S5_DetalleTransaccionAudit'
)
BEGIN
    ALTER TABLE [DetalleTransaccion] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194205_S5_DetalleTransaccionAudit'
)
BEGIN
    ALTER TABLE [DetalleTransaccion] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194205_S5_DetalleTransaccionAudit'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609194205_S5_DetalleTransaccionAudit', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194505_S6_MetodoPagoEsEfectivo'
)
BEGIN
    ALTER TABLE [MetodoPago] ADD [EsEfectivo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194505_S6_MetodoPagoEsEfectivo'
)
BEGIN
    UPDATE MetodoPago SET EsEfectivo = 1 WHERE Nombre = 'Efectivo'
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609194505_S6_MetodoPagoEsEfectivo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609194505_S6_MetodoPagoEsEfectivo', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609195753_S7_TransaccionSunatEstado'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [SunatError] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609195753_S7_TransaccionSunatEstado'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [SunatEstado] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609195753_S7_TransaccionSunatEstado'
)
BEGIN
    UPDATE Transaccion SET SunatEstado = 'Emitida' WHERE SunatEstado IS NULL OR SunatEstado = ''
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609195753_S7_TransaccionSunatEstado'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609195753_S7_TransaccionSunatEstado', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609202855_S8_OperadorUsuarioFK'
)
BEGIN
    ALTER TABLE [Operador] ADD [UsuarioId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609202855_S8_OperadorUsuarioFK'
)
BEGIN
    CREATE INDEX [IX_Operador_UsuarioId] ON [Operador] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609202855_S8_OperadorUsuarioFK'
)
BEGIN
    ALTER TABLE [Operador] ADD CONSTRAINT [FK_Operador_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([UsuarioId]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609202855_S8_OperadorUsuarioFK'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609202855_S8_OperadorUsuarioFK', N'9.0.17');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610000300_S9_TransaccionSunatNumeroSerie'
)
BEGIN
    ALTER TABLE [Transaccion] ADD [SunatNumeroSerie] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610000300_S9_TransaccionSunatNumeroSerie'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610000300_S9_TransaccionSunatNumeroSerie', N'9.0.17');
END;

COMMIT;
GO

