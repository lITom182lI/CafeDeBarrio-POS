-- PASO 6: DDL - Basado en el modelado_tablas_final
-- Creación de Base de Datos y Tablas

USE master;
GO

IF DB_ID('CafeDeBarrio') IS NOT NULL
BEGIN
    -- Desconectar a los usuarios activos para poder borrarla
    ALTER DATABASE CafeDeBarrio SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CafeDeBarrio;
END
GO

CREATE DATABASE CafeDeBarrio;
GO

USE CafeDeBarrio;
GO

-- 1. Tablas Base sin Claves Foráneas

CREATE TABLE SEDE (
    IdSede INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    direccion VARCHAR(200),
    Ciudad VARCHAR(100),
    telefono VARCHAR(20),
    Habilitado BIT DEFAULT 1
);
GO

CREATE TABLE Turno (
    IdTurno INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL,
    Horas_semanales INT
);
GO

CREATE TABLE Cargo (
    IdCargo INT PRIMARY KEY IDENTITY(1,1),
    Nombre_cargo VARCHAR(100) NOT NULL,
    Suedo_base DECIMAL(10,2),
    Descripcion VARCHAR(255)
);
GO

CREATE TABLE TIPO_CLIENTE (
    IdTipo_cliente INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(255)
);
GO

CREATE TABLE COMPROBANTE (
    IdComprobante INT PRIMARY KEY IDENTITY(1,1),
    fecha_emision DATE,
    tipo_comprobante VARCHAR(50)
);
GO

CREATE TABLE TIPO_PAGO (
    IdTipoPago INT PRIMARY KEY IDENTITY(1,1),
    NombrePago VARCHAR(50) NOT NULL
);
GO

CREATE TABLE TIPO_DESCUENTO (
    IdDescuento INT PRIMARY KEY IDENTITY(1,1),
    porc_descuento DECIMAL(5,2),
    descripcion VARCHAR(255),
    tipo_descuento VARCHAR(50)
);
GO

CREATE TABLE EMPRESA_TRANSPORTE (
    IdTransporte INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    tipo_envio VARCHAR(50),
    cobertura VARCHAR(100),
    telefono VARCHAR(20)
);
GO

CREATE TABLE PRODUCTO (
    IdProducto INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    precio DECIMAL(10,2) NOT NULL,
    costo DECIMAL(10,2) NOT NULL,
    existencia INT NOT NULL DEFAULT 0,
    marca VARCHAR(100),
    origen VARCHAR(100),
    nivel_tostado VARCHAR(50),
    cantidad_unidad INT,
    peso_presentacion VARCHAR(50)
);
GO

CREATE TABLE PROVEEDOR (
    Id_Proveedor INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    RUC VARCHAR(20) NOT NULL UNIQUE,
    telefono VARCHAR(20),
    ciudad VARCHAR(100),
    tiempo_entrega VARCHAR(50)
);
GO

CREATE TABLE ORIGEN (
    IdOrigen INT PRIMARY KEY IDENTITY(1,1),
    Pais VARCHAR(100),
    Region VARCHAR(100)
);
GO


-- 2. Tablas Dependientes (Con Claves Foráneas)

CREATE TABLE EMPLEADO (
    IdEmpleado INT PRIMARY KEY IDENTITY(1,1),
    IdSede INT NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    apellido_paterno VARCHAR(100) NOT NULL,
    apellido_materno VARCHAR(100),
    fecha_inicio DATE,
    IdTurno INT NOT NULL,
    estado VARCHAR(50),
    Sueldo DECIMAL(10,2),
    IdCargo INT NOT NULL,
    Supervisor INT NULL,
    CONSTRAINT FK_Empleado_Sede FOREIGN KEY (IdSede) REFERENCES SEDE(IdSede),
    CONSTRAINT FK_Empleado_Turno FOREIGN KEY (IdTurno) REFERENCES Turno(IdTurno),
    CONSTRAINT FK_Empleado_Cargo FOREIGN KEY (IdCargo) REFERENCES Cargo(IdCargo),
    CONSTRAINT FK_Empleado_Supervisor FOREIGN KEY (Supervisor) REFERENCES EMPLEADO(IdEmpleado)
);
GO

CREATE TABLE CLIENTE (
    IdCliente INT PRIMARY KEY IDENTITY(1,1),
    Nombre VARCHAR(100) NOT NULL,
    apellido_paterno VARCHAR(100),
    apellido_materno VARCHAR(100),
    NroDocumento VARCHAR(20) NOT NULL UNIQUE,
    tipo_documento VARCHAR(20),
    telefono VARCHAR(20),
    IdTipo_cliente INT NOT NULL,
    Habilitado BIT DEFAULT 1,
    CONSTRAINT FK_Cliente_TipoCliente FOREIGN KEY (IdTipo_cliente) REFERENCES TIPO_CLIENTE(IdTipo_cliente)
);
GO

CREATE TABLE VENTA (
    NroVenta INT PRIMARY KEY IDENTITY(1,1),
    IdCliente INT NOT NULL,
    IdComprobante INT NOT NULL,
    FechaHora DATETIME NOT NULL,
    CONSTRAINT FK_Venta_Cliente FOREIGN KEY (IdCliente) REFERENCES CLIENTE(IdCliente),
    CONSTRAINT FK_Venta_Comprobante FOREIGN KEY (IdComprobante) REFERENCES COMPROBANTE(IdComprobante)
);
GO

CREATE TABLE COBRA (
    NroVenta INT NOT NULL,
    IdEmpleado INT NOT NULL,
    IdTipoPago INT NOT NULL,
    MontoPago DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (NroVenta, IdEmpleado, IdTipoPago),
    CONSTRAINT FK_Cobra_Venta FOREIGN KEY (NroVenta) REFERENCES VENTA(NroVenta),
    CONSTRAINT FK_Cobra_Empleado FOREIGN KEY (IdEmpleado) REFERENCES EMPLEADO(IdEmpleado),
    CONSTRAINT FK_Cobra_TipoPago FOREIGN KEY (IdTipoPago) REFERENCES TIPO_PAGO(IdTipoPago)
);
GO

CREATE TABLE APLICA_DESCUENTO (
    NroVenta INT NOT NULL,
    IdEmpleado INT NOT NULL,
    IdDescuento INT NOT NULL,
    PRIMARY KEY (NroVenta, IdEmpleado, IdDescuento),
    CONSTRAINT FK_Aplica_Venta FOREIGN KEY (NroVenta) REFERENCES VENTA(NroVenta),
    CONSTRAINT FK_Aplica_Empleado FOREIGN KEY (IdEmpleado) REFERENCES EMPLEADO(IdEmpleado),
    CONSTRAINT FK_Aplica_Descuento FOREIGN KEY (IdDescuento) REFERENCES TIPO_DESCUENTO(IdDescuento)
);
GO

CREATE TABLE ENVIA (
    NroVenta INT NOT NULL,
    IdTransporte INT NOT NULL,
    fecha_envio DATE,
    estado_envio VARCHAR(50),
    fecha_entrega_estimado DATE,
    nro_guia VARCHAR(50),
    costo_envio DECIMAL(10,2),
    PRIMARY KEY (NroVenta, IdTransporte),
    CONSTRAINT FK_Envia_Venta FOREIGN KEY (NroVenta) REFERENCES VENTA(NroVenta),
    CONSTRAINT FK_Envia_Transporte FOREIGN KEY (IdTransporte) REFERENCES EMPRESA_TRANSPORTE(IdTransporte)
);
GO

CREATE TABLE DETALLE_VENTA (
    NroVenta INT NOT NULL,
    IdProducto INT NOT NULL,
    cantidad INT NOT NULL,
    precio_venta DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (NroVenta, IdProducto),
    CONSTRAINT FK_Detalle_Venta FOREIGN KEY (NroVenta) REFERENCES VENTA(NroVenta),
    CONSTRAINT FK_Detalle_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto)
);
GO

CREATE TABLE SUMINISTRA (
    Id_Proveedor INT NOT NULL,
    IdProducto INT NOT NULL,
    cant_min_pedido INT,
    estado_suministro VARCHAR(50),
    precio_unitario DECIMAL(10,2),
    fecha_act_precio DATE,
    PRIMARY KEY (Id_Proveedor, IdProducto),
    CONSTRAINT FK_Suministra_Proveedor FOREIGN KEY (Id_Proveedor) REFERENCES PROVEEDOR(Id_Proveedor),
    CONSTRAINT FK_Suministra_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto)
);
GO

CREATE TABLE CAFE_GRANO (
    IdProducto INT PRIMARY KEY,
    tipo_grano VARCHAR(100),
    proceso VARCHAR(100),
    CONSTRAINT FK_CafeGrano_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto)
);
GO

CREATE TABLE CAFE_MOLIDO (
    IdProducto INT PRIMARY KEY,
    tipo_molienda VARCHAR(100),
    CONSTRAINT FK_CafeMolido_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto)
);
GO

CREATE TABLE DEVOLUCION (
    Nro_Devolucion INT PRIMARY KEY IDENTITY(1,1),
    NroVenta INT NOT NULL,
    motivo_Devolucion VARCHAR(255),
    fechaHora DATETIME,
    estado_devolucion VARCHAR(50),
    IdEmpleado INT NOT NULL,
    CONSTRAINT FK_Devolucion_Venta FOREIGN KEY (NroVenta) REFERENCES VENTA(NroVenta),
    CONSTRAINT FK_Devolucion_Empleado FOREIGN KEY (IdEmpleado) REFERENCES EMPLEADO(IdEmpleado)
);
GO

CREATE TABLE CONTIENE (
    Nro_Devolucion INT NOT NULL,
    IdProducto INT NOT NULL,
    precio_unitario_devuelto DECIMAL(10,2),
    cantidad_devuelta INT,
    PRIMARY KEY (Nro_Devolucion, IdProducto),
    CONSTRAINT FK_Contiene_Devolucion FOREIGN KEY (Nro_Devolucion) REFERENCES DEVOLUCION(Nro_Devolucion),
    CONSTRAINT FK_Contiene_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto)
);
GO

CREATE TABLE PROVIENE (
    IdProducto INT NOT NULL,
    IdOrigen INT NOT NULL,
    PRIMARY KEY (IdProducto, IdOrigen),
    CONSTRAINT FK_Proviene_Producto FOREIGN KEY (IdProducto) REFERENCES PRODUCTO(IdProducto),
    CONSTRAINT FK_Proviene_Origen FOREIGN KEY (IdOrigen) REFERENCES ORIGEN(IdOrigen)
);
GO
