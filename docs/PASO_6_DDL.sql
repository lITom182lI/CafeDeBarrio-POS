-- ============================================================
-- PASO 6 — IMPLEMENTACIÓN DDL
-- Sistema: Café de Barrio POS
-- Motor:   Microsoft SQL Server (T-SQL)
-- Alumnos: Pablo Joel Castillo Flores, Justhin Christofher Huisa Valle, Jeremy Geraldo Armas Camones, Geradth Humberto Gaitan Gonzales, Allison Isabel Cordova Diaz
-- Fecha:   2026-06-15
-- Descripción: Definición de las 14 tablas del dominio de
--              negocio con constraints completos (PK, FK,
--              NOT NULL, UNIQUE, CHECK) en orden de dependencia.
-- ============================================================

-- Precaución: ejecutar en una base de datos vacía o de prueba.
-- Para recrear desde cero, eliminar tablas en orden inverso
-- antes de ejecutar.

IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'CafeDeBarrioBD')
BEGIN
    CREATE DATABASE CafeDeBarrioBD;
END
GO

USE CafeDeBarrioBD;
GO

-- ============================================================
-- NIVEL 0 — Tablas sin dependencias externas
-- ============================================================

-- 1. CategoriaCafe
--    Agrupa los productos por tipo (Cafés, Bebidas, Comida…)
CREATE TABLE CategoriaCafe (
    categoria_id    INT           IDENTITY(1,1) NOT NULL,
    codigo          NVARCHAR(10)  NOT NULL,
    nombre          NVARCHAR(100) NOT NULL,
    descripcion     NVARCHAR(300) NULL,
    activa          BIT           NOT NULL  DEFAULT 1,
    CONSTRAINT PK_CategoriaCafe PRIMARY KEY (categoria_id),
    CONSTRAINT UQ_CategoriaCafe_Codigo UNIQUE (codigo)
);
GO

-- 2. TipoCliente
--    Clasifica clientes: Regular, Frecuente, Mayorista
CREATE TABLE TipoCliente (
    tipo_cliente_id INT           IDENTITY(1,1) NOT NULL,
    nombre          NVARCHAR(50)  NOT NULL,
    descripcion     NVARCHAR(200) NULL,
    CONSTRAINT PK_TipoCliente PRIMARY KEY (tipo_cliente_id)
);
GO

-- 3. Sede
--    Sucursales físicas del negocio
CREATE TABLE Sede (
    sede_id        INT           IDENTITY(1,1) NOT NULL,
    nombre         NVARCHAR(100) NOT NULL,
    direccion      NVARCHAR(255) NOT NULL,
    distrito       NVARCHAR(100) NOT NULL,
    ciudad         NVARCHAR(100) NOT NULL,
    telefono       NVARCHAR(20)  NULL,
    es_principal   BIT           NOT NULL  DEFAULT 0,
    activa         BIT           NOT NULL  DEFAULT 1,
    fecha_apertura DATE          NULL,
    CONSTRAINT PK_Sede PRIMARY KEY (sede_id)
);
GO

-- 4. MetodoPago
--    Formas de pago aceptadas (Efectivo, Tarjeta, Yape…)
CREATE TABLE MetodoPago (
    metodo_pago_id INT           IDENTITY(1,1) NOT NULL,
    nombre         NVARCHAR(100) NOT NULL,
    activo         BIT           NOT NULL  DEFAULT 1,
    EsEfectivo     BIT           NOT NULL  DEFAULT 0,
    CONSTRAINT PK_MetodoPago PRIMARY KEY (metodo_pago_id)
);
GO

-- 5. OpcionEnvio
--    Modalidades de entrega (Recojo, Delivery Express…)
CREATE TABLE OpcionEnvio (
    opcion_envio_id INT           IDENTITY(1,1) NOT NULL,
    nombre          NVARCHAR(100) NOT NULL,
    descripcion     NVARCHAR(300) NULL,
    tarifa          DECIMAL(18,2) NOT NULL,
    activa          BIT           NOT NULL  DEFAULT 1,
    CONSTRAINT PK_OpcionEnvio PRIMARY KEY (opcion_envio_id),
    CONSTRAINT CK_OpcionEnvio_Tarifa CHECK (tarifa >= 0)
);
GO

-- ============================================================
-- NIVEL 1 — Dependen de tablas de Nivel 0
-- ============================================================

-- 6. ConfiguracionNegocio
--    Tasas impositivas vigentes por sede (IGV=16%, IPM=2%)
--    Invariante: solo puede haber UNA configuración activa por sede
CREATE TABLE ConfiguracionNegocio (
    ConfiguracionNegocioId INT           IDENTITY(1,1) NOT NULL,
    SedeId                 INT           NOT NULL,
    TasaIGV                DECIMAL(5,4)  NOT NULL,
    TasaIPM                DECIMAL(5,4)  NOT NULL,
    FechaVigencia          DATETIME2     NOT NULL,
    Activo                 BIT           NOT NULL  DEFAULT 1,
    CONSTRAINT PK_ConfiguracionNegocio PRIMARY KEY (ConfiguracionNegocioId),
    CONSTRAINT FK_ConfiguracionNegocio_Sede
        FOREIGN KEY (SedeId) REFERENCES Sede(sede_id)
        ON DELETE NO ACTION,
    -- Solo una configuración activa por sede
    CONSTRAINT CK_ConfiguracionNegocio_TasaIGV CHECK (TasaIGV > 0 AND TasaIGV < 1),
    CONSTRAINT CK_ConfiguracionNegocio_TasaIPM CHECK (TasaIPM >= 0 AND TasaIPM < 1)
);
GO

-- Índice filtrado: garantiza unicidad de configuración activa por sede
CREATE UNIQUE INDEX UX_ConfiguracionNegocio_SedeId_Activa
    ON ConfiguracionNegocio(SedeId)
    WHERE Activo = 1;
GO

-- 7. Cliente
--    Compradores registrados; el email es único en el sistema
CREATE TABLE Cliente (
    cliente_id      INT           IDENTITY(1,1) NOT NULL,
    tipo_cliente_id INT           NOT NULL,
    nombre          NVARCHAR(100) NOT NULL,
    apellido        NVARCHAR(100) NOT NULL,
    email           NVARCHAR(255) NOT NULL,
    codigo_cliente  NVARCHAR(20)  NULL,
    tipo_documento  NVARCHAR(20)  NULL,
    numero_documento NVARCHAR(50) NULL,
    telefono        NVARCHAR(20)  NULL,
    direccion       NVARCHAR(300) NULL,
    distrito        NVARCHAR(100) NULL,
    ciudad          NVARCHAR(100) NULL,
    fecha_registro  DATE          NOT NULL,
    activo          BIT           NOT NULL  DEFAULT 1,
    CONSTRAINT PK_Cliente PRIMARY KEY (cliente_id),
    CONSTRAINT FK_Cliente_TipoCliente
        FOREIGN KEY (tipo_cliente_id) REFERENCES TipoCliente(tipo_cliente_id)
        ON DELETE CASCADE,
    CONSTRAINT UQ_Cliente_Email UNIQUE (email)
);
GO

-- 8. Producto
--    Catálogo de ítems vendibles con precio IGV-inclusivo
--    Invariante: precio >= 0; costo >= 0
CREATE TABLE Producto (
    producto_id          INT           IDENTITY(1,1) NOT NULL,
    categoria_id         INT           NOT NULL,
    nombre               NVARCHAR(200) NOT NULL,
    descripcion          NVARCHAR(MAX) NULL,
    costo                DECIMAL(18,2) NOT NULL,
    precio               DECIMAL(18,2) NOT NULL,
    cantidad_disponible  DECIMAL(10,3) NOT NULL  DEFAULT 0,
    stock_minimo         DECIMAL(10,3) NOT NULL  DEFAULT 0,
    unidad_medida        NVARCHAR(20)  NOT NULL  DEFAULT 'unidad',
    seguimiento_inventario BIT         NOT NULL  DEFAULT 1,
    cantidad_por_unidad  NVARCHAR(50)  NULL,
    imagen_url           NVARCHAR(500) NULL,
    es_mayorista         BIT           NOT NULL  DEFAULT 0,
    activo               BIT           NOT NULL  DEFAULT 1,
    created_at           DATETIME2     NOT NULL,
    updated_at           DATETIME2     NULL,
    -- RowVersion: token de concurrencia optimista (EF Core lo gestiona)
    RowVersion           ROWVERSION    NOT NULL,
    CONSTRAINT PK_Producto PRIMARY KEY (producto_id),
    CONSTRAINT FK_Producto_CategoriaCafe
        FOREIGN KEY (categoria_id) REFERENCES CategoriaCafe(categoria_id)
        ON DELETE CASCADE,
    CONSTRAINT CK_Producto_Precio_Positivo CHECK (precio >= 0),
    CONSTRAINT CK_Producto_Costo_Positivo  CHECK (costo >= 0)
);
GO

-- 9. Operador
--    Personal de caja que atiende turnos (PIN protegido con Argon2)
CREATE TABLE Operador (
    OperadorId       INT           IDENTITY(1,1) NOT NULL,
    SedeId           INT           NOT NULL,
    Nombre           NVARCHAR(100) NOT NULL,
    PinHash          NVARCHAR(256) NOT NULL,
    Activo           BIT           NOT NULL  DEFAULT 1,
    Eliminado        BIT           NOT NULL  DEFAULT 0,
    FailedPinAttempts INT          NOT NULL  DEFAULT 0,
    IsLockedOut      BIT           NOT NULL  DEFAULT 0,
    LockedUntilUtc   DATETIME2     NULL,
    CreatedAt        DATETIME2     NOT NULL,
    UpdatedAt        DATETIME2     NULL,
    CONSTRAINT PK_Operador PRIMARY KEY (OperadorId),
    CONSTRAINT FK_Operador_Sede
        FOREIGN KEY (SedeId) REFERENCES Sede(sede_id)
        ON DELETE NO ACTION
);
GO

-- ============================================================
-- NIVEL 2 — Dependen de Nivel 1
-- ============================================================

-- 10. Turno
--     Período de trabajo de un operador: apertura → cierre de caja
--     Estado: 'Abierto' | 'Cerrado'
CREATE TABLE Turno (
    TurnoId                  INT           IDENTITY(1,1) NOT NULL,
    SedeId                   INT           NOT NULL,
    OperadorId               INT           NOT NULL,
    FechaApertura            DATETIME2     NOT NULL,
    FechaCierre              DATETIME2     NULL,
    MontoApertura            DECIMAL(10,2) NOT NULL,
    MontoEfectivoCierto      DECIMAL(10,2) NULL,
    TotalEfectivoSistema     DECIMAL(10,2) NULL,
    TotalVentasEfectivo      DECIMAL(18,2) NOT NULL  DEFAULT 0,
    TotalAnulacionesEfectivo DECIMAL(18,2) NOT NULL  DEFAULT 0,
    TotalMovimientosEntrada  DECIMAL(18,2) NOT NULL  DEFAULT 0,
    TotalMovimientosSalida   DECIMAL(18,2) NOT NULL  DEFAULT 0,
    SaldoEsperado            DECIMAL(18,2) NOT NULL  DEFAULT 0,
    Diferencia               DECIMAL(18,2) NULL,
    Estado                   NVARCHAR(20)  NOT NULL  DEFAULT 'Abierto',
    Observaciones            NVARCHAR(500) NULL,
    RowVersion               ROWVERSION    NOT NULL,
    CreatedAt                DATETIME2     NOT NULL,
    UpdatedAt                DATETIME2     NULL,
    CONSTRAINT PK_Turno PRIMARY KEY (TurnoId),
    CONSTRAINT FK_Turno_Sede
        FOREIGN KEY (SedeId) REFERENCES Sede(sede_id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Turno_Operador
        FOREIGN KEY (OperadorId) REFERENCES Operador(OperadorId)
        ON DELETE NO ACTION,
    CONSTRAINT CK_Turno_Estado CHECK (Estado IN ('Abierto', 'Cerrado'))
);
GO

-- ============================================================
-- NIVEL 3 — Dependen de Nivel 2
-- ============================================================

-- 11. MovimientoCaja
--     Retiros o ingresos de efectivo durante un turno
--     TipoMovimiento: 'Entrada' | 'Salida'
CREATE TABLE MovimientoCaja (
    MovimientoCajaId INT           IDENTITY(1,1) NOT NULL,
    TurnoId          INT           NOT NULL,
    TipoMovimiento   NVARCHAR(20)  NOT NULL,
    Motivo           NVARCHAR(200) NOT NULL,
    Monto            DECIMAL(10,2) NOT NULL,
    FechaHora        DATETIME2     NOT NULL,
    CreatedAt        DATETIME2     NOT NULL,
    UpdatedAt        DATETIME2     NULL,
    CONSTRAINT PK_MovimientoCaja PRIMARY KEY (MovimientoCajaId),
    CONSTRAINT FK_MovimientoCaja_Turno
        FOREIGN KEY (TurnoId) REFERENCES Turno(TurnoId)
        ON DELETE NO ACTION,
    CONSTRAINT CK_MovimientoCaja_Tipo   CHECK (TipoMovimiento IN ('Entrada', 'Salida')),
    CONSTRAINT CK_MovimientoCaja_Monto  CHECK (Monto > 0)
);
GO

-- 12. Transaccion
--     Venta registrada en el POS.
--     Modelo IGV extractivo: precio es IGV-inclusivo.
--     subtotal = ROUND(total / 1.18, 2)  →  base imponible
--     impuesto = ROUND(total - subtotal, 2)
CREATE TABLE Transaccion (
    transaccion_id           INT           IDENTITY(1,1) NOT NULL,
    cliente_id               INT           NULL,
    sede_id                  INT           NOT NULL,
    metodo_pago_id           INT           NOT NULL,
    metodo_pago_secundario_id INT          NULL,
    monto_metodo_primario    DECIMAL(18,2) NULL,
    opcion_envio_id          INT           NULL,
    turno_id                 INT           NULL,
    operador_id              INT           NULL,
    es_mayorista             BIT           NOT NULL  DEFAULT 0,
    canal                    NVARCHAR(20)  NOT NULL  DEFAULT 'POS',
    fecha                    DATETIME2     NOT NULL,
    subtotal                 DECIMAL(18,2) NOT NULL,
    impuesto                 DECIMAL(18,2) NOT NULL,
    recargo_propina          DECIMAL(10,2) NOT NULL  DEFAULT 0,
    costo_envio              DECIMAL(18,2) NOT NULL  DEFAULT 0,
    total                    DECIMAL(18,2) NOT NULL,
    notas                    NVARCHAR(MAX) NULL,
    tipo_documento           NVARCHAR(20)  NULL,
    numero_documento         NVARCHAR(20)  NULL,
    razon_social             NVARCHAR(150) NULL,
    SunatEstado              NVARCHAR(50)  NOT NULL  DEFAULT 'Pendiente',
    SunatError               NVARCHAR(MAX) NULL,
    SunatNumeroSerie         NVARCHAR(50)  NULL,
    SunatIntentos            INT           NOT NULL  DEFAULT 0,
    created_at               DATETIME2     NOT NULL,
    updated_at               DATETIME2     NULL,
    RowVersion               ROWVERSION    NOT NULL,
    CONSTRAINT PK_Transaccion PRIMARY KEY (transaccion_id),
    CONSTRAINT FK_Transaccion_Cliente
        FOREIGN KEY (cliente_id) REFERENCES Cliente(cliente_id)
        ON DELETE SET NULL,
    CONSTRAINT FK_Transaccion_Sede
        FOREIGN KEY (sede_id) REFERENCES Sede(sede_id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Transaccion_MetodoPago
        FOREIGN KEY (metodo_pago_id) REFERENCES MetodoPago(metodo_pago_id)
        ON DELETE CASCADE,
    CONSTRAINT FK_Transaccion_MetodoPagoSecundario
        FOREIGN KEY (metodo_pago_secundario_id) REFERENCES MetodoPago(metodo_pago_id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Transaccion_OpcionEnvio
        FOREIGN KEY (opcion_envio_id) REFERENCES OpcionEnvio(opcion_envio_id),
    CONSTRAINT FK_Transaccion_Turno
        FOREIGN KEY (turno_id) REFERENCES Turno(TurnoId)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Transaccion_Operador
        FOREIGN KEY (operador_id) REFERENCES Operador(OperadorId)
        ON DELETE NO ACTION,
    -- Integridad financiera: subtotal y total deben ser positivos
    CONSTRAINT CK_Transaccion_Subtotal_Positivo CHECK (subtotal >= 0),
    CONSTRAINT CK_Transaccion_Total_Positivo    CHECK (total >= 0),
    -- total debe cubrir al menos la base + impuesto (puede tener propina/envío)
    CONSTRAINT CK_Transaccion_Total_Coherente   CHECK (total >= subtotal + impuesto)
);
GO

-- Índice compuesto para reportes por sede y rango de fechas
CREATE INDEX IX_Transaccion_SedeId_Fecha ON Transaccion(sede_id, fecha);
GO
CREATE INDEX IX_Transacciones_Fecha ON Transaccion(fecha);
GO

-- ============================================================
-- NIVEL 4 — Dependen de Transaccion y Producto
-- ============================================================

-- 13. DetalleTransaccion
--     Líneas de venta (tabla puente N:M entre Transaccion y Producto).
--     subtotal_linea = ROUND((precio / 1.18) * cantidad, 2)  →  base pre-IGV
CREATE TABLE DetalleTransaccion (
    detalle_id      INT           IDENTITY(1,1) NOT NULL,
    transaccion_id  INT           NOT NULL,
    producto_id     INT           NOT NULL,
    cantidad        INT           NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    subtotal_linea  DECIMAL(18,2) NOT NULL,
    CreatedAt       DATETIME2     NOT NULL,
    UpdatedAt       DATETIME2     NULL,
    CONSTRAINT PK_DetalleTransaccion PRIMARY KEY (detalle_id),
    CONSTRAINT FK_DetalleTransaccion_Transaccion
        FOREIGN KEY (transaccion_id) REFERENCES Transaccion(transaccion_id)
        ON DELETE CASCADE,
    CONSTRAINT FK_DetalleTransaccion_Producto
        FOREIGN KEY (producto_id) REFERENCES Producto(producto_id)
        ON DELETE CASCADE,
    CONSTRAINT CK_DetalleTransaccion_Cantidad CHECK (cantidad > 0),
    CONSTRAINT CK_DetalleTransaccion_Precio   CHECK (precio_unitario >= 0),
    CONSTRAINT CK_DetalleTransaccion_Subtotal CHECK (subtotal_linea >= 0)
);
GO

-- 14. Anulacion
--     Reversa de una transacción; relación 1:1 con Transaccion.
--     TipoAnulacion: 'DevolucionTotal' | 'DevolucionParcial' | 'Cancelacion'
CREATE TABLE Anulacion (
    AnulacionId           INT           IDENTITY(1,1) NOT NULL,
    TransaccionId         INT           NOT NULL,
    TipoAnulacion         NVARCHAR(20)  NOT NULL,
    Motivo                NVARCHAR(200) NOT NULL,
    MontoOriginal         DECIMAL(10,2) NOT NULL,
    MontoDevuelto         DECIMAL(10,2) NOT NULL,
    MetodoDevolucion      NVARCHAR(50)  NULL,
    OperadorSolicitanteId INT           NOT NULL,
    AutorizadorId         INT           NOT NULL,
    FechaHora             DATETIME2     NOT NULL,
    ImpactoInventario     BIT           NOT NULL  DEFAULT 1,
    CreatedAt             DATETIME2     NOT NULL,
    UpdatedAt             DATETIME2     NULL,
    CONSTRAINT PK_Anulacion PRIMARY KEY (AnulacionId),
    -- Una transacción solo puede anularse una vez (1:1)
    CONSTRAINT UQ_Anulacion_TransaccionId UNIQUE (TransaccionId),
    CONSTRAINT FK_Anulacion_Transaccion
        FOREIGN KEY (TransaccionId) REFERENCES Transaccion(transaccion_id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Anulacion_OperadorSolicitante
        FOREIGN KEY (OperadorSolicitanteId) REFERENCES Operador(OperadorId)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Anulacion_Autorizador
        FOREIGN KEY (AutorizadorId) REFERENCES Operador(OperadorId)
        ON DELETE NO ACTION,
    CONSTRAINT CK_Anulacion_MontoDevuelto
        CHECK (MontoDevuelto >= 0 AND MontoDevuelto <= MontoOriginal),
    CONSTRAINT CK_Anulacion_Tipo
        CHECK (TipoAnulacion IN ('DevolucionTotal', 'DevolucionParcial', 'Cancelacion'))
);
GO

-- ============================================================
-- FIN DEL SCRIPT DDL — 14 tablas creadas
-- ============================================================
