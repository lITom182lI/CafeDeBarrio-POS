-- PASO 6: DML - Datos de Prueba para CafeDeBarrio

USE CafeDeBarrio;
GO

-- 1. Inserciones en Tablas Base

INSERT INTO SEDE (Nombre, direccion, Ciudad, telefono, Habilitado)
VALUES 
('Sede Central', 'Av. Principal 123', 'Lima', '01-1234567', 1),
('Sede Norte', 'Av. Las Palmas 456', 'Lima', '01-7654321', 1);
GO

INSERT INTO Turno (Nombre, Horas_semanales)
VALUES 
('Mañana', 40),
('Tarde', 40),
('Noche', 30);
GO

INSERT INTO Cargo (Nombre_cargo, Suedo_base, Descripcion)
VALUES 
('Gerente', 3500.00, 'Gerente de sede'),
('Barista', 1500.00, 'Preparación de café'),
('Cajero', 1200.00, 'Atención en caja');
GO

INSERT INTO TIPO_CLIENTE (Nombre, Descripcion)
VALUES 
('Regular', 'Cliente ocasional'),
('VIP', 'Cliente frecuente con beneficios');
GO

-- IDENTITY_INSERT no es necesario si omitimos la PK, pero dado que en Comprobante y otros campos IDENTITY no queremos lidiar con valores que no conocemos, usamos los normales o permitimos que IDENTITY asigne solos.
INSERT INTO COMPROBANTE (fecha_emision, tipo_comprobante)
VALUES 
('2023-10-01', 'Boleta'),
('2023-10-02', 'Factura');
GO

INSERT INTO TIPO_PAGO (NombrePago)
VALUES 
('Efectivo'),
('Tarjeta de Crédito'),
('Yape'),
('Plin');
GO

INSERT INTO TIPO_DESCUENTO (porc_descuento, descripcion, tipo_descuento)
VALUES 
(10.00, 'Descuento VIP', 'Porcentaje'),
(0.00, 'Sin Descuento', 'Ninguno');
GO

INSERT INTO EMPRESA_TRANSPORTE (nombre, tipo_envio, cobertura, telefono)
VALUES 
('Delivery Express', 'Motorizado', 'Local', '987654321'),
('Olva', 'Courier', 'Nacional', '01-9876543');
GO

INSERT INTO PRODUCTO (nombre, precio, costo, existencia, marca, origen, nivel_tostado, cantidad_unidad, peso_presentacion)
VALUES 
('Café Americano', 8.00, 3.00, 100, 'Propia', 'Perú', 'Medio', 1, '8 oz'),
('Bolsa Café en Grano', 45.00, 20.00, 50, 'CafeDeBarrio', 'Cusco', 'Oscuro', 1, '250g'),
('Bolsa Café Molido', 48.00, 22.00, 40, 'CafeDeBarrio', 'Cajamarca', 'Medio', 1, '250g');
GO

INSERT INTO PROVEEDOR (nombre, RUC, telefono, ciudad, tiempo_entrega)
VALUES 
('Finca El Cafetal', '20123456789', '999888777', 'Cusco', '3 días'),
('Distribuidora Aromas', '20987654321', '988777666', 'Lima', '1 día');
GO

INSERT INTO ORIGEN (Pais, Region)
VALUES 
('Perú', 'Cusco'),
('Perú', 'Cajamarca'),
('Colombia', 'Eje Cafetero');
GO

-- 2. Inserciones en Tablas Dependientes

-- Obtenemos IDs generados asumiendo que empiezan en 1
INSERT INTO EMPLEADO (IdSede, Nombre, apellido_paterno, apellido_materno, fecha_inicio, IdTurno, estado, Sueldo, IdCargo, Supervisor)
VALUES 
(1, 'Carlos', 'Gomez', 'Perez', '2020-01-15', 1, 'Activo', 3500.00, 1, NULL),
(1, 'Ana', 'Lopez', 'Rios', '2021-03-10', 1, 'Activo', 1500.00, 2, 1),
(1, 'Luis', 'Torres', 'Mendoza', '2022-05-20', 2, 'Activo', 1200.00, 3, 1);
GO

INSERT INTO CLIENTE (Nombre, apellido_paterno, apellido_materno, NroDocumento, tipo_documento, telefono, IdTipo_cliente, Habilitado)
VALUES 
('Maria', 'Vargas', 'Silva', '70123456', 'DNI', '912345678', 1, 1),
('Juan', 'Salas', 'Rojas', '70654321', 'DNI', '923456789', 2, 1);
GO

INSERT INTO VENTA (IdCliente, IdComprobante, FechaHora)
VALUES 
(1, 1, '2023-10-01 10:30:00'),
(2, 2, '2023-10-02 15:45:00');
GO

INSERT INTO DETALLE_VENTA (NroVenta, IdProducto, cantidad, precio_venta)
VALUES 
(1, 1, 2, 8.00),
(2, 2, 1, 45.00);
GO

INSERT INTO COBRA (NroVenta, IdEmpleado, IdTipoPago, MontoPago)
VALUES 
(1, 3, 3, 16.00), -- Yape
(2, 3, 2, 45.00); -- Tarjeta de Crédito
GO

INSERT INTO APLICA_DESCUENTO (NroVenta, IdEmpleado, IdDescuento)
VALUES 
(2, 3, 1); -- Descuento VIP al cliente 2
GO

INSERT INTO ENVIA (NroVenta, IdTransporte, fecha_envio, estado_envio, fecha_entrega_estimado, nro_guia, costo_envio)
VALUES 
(2, 1, '2023-10-02', 'En Camino', '2023-10-02', 'G-001', 5.00);
GO

INSERT INTO SUMINISTRA (Id_Proveedor, IdProducto, cant_min_pedido, estado_suministro, precio_unitario, fecha_act_precio)
VALUES 
(1, 2, 10, 'Activo', 20.00, '2023-01-01'),
(1, 3, 10, 'Activo', 22.00, '2023-01-01');
GO

-- Los productos 2 y 3 son de grano y molido respectivamente
INSERT INTO CAFE_GRANO (IdProducto, tipo_grano, proceso)
VALUES 
(2, 'Arábica', 'Lavado');
GO

INSERT INTO CAFE_MOLIDO (IdProducto, tipo_molienda)
VALUES 
(3, 'Fina');
GO

INSERT INTO DEVOLUCION (NroVenta, motivo_Devolucion, fechaHora, estado_devolucion, IdEmpleado)
VALUES 
(1, 'Bebida fría', '2023-10-01 10:35:00', 'Procesado', 1);
GO

INSERT INTO CONTIENE (Nro_Devolucion, IdProducto, precio_unitario_devuelto, cantidad_devuelta)
VALUES 
(1, 1, 8.00, 1);
GO

INSERT INTO PROVIENE (IdProducto, IdOrigen)
VALUES 
(2, 1),
(3, 2);
GO
