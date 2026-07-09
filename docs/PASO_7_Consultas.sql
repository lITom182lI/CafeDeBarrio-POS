-- PASO 7: Consultas - Basado en el modelo de tablas final

USE CafeDeBarrio;
GO

-- 1. Listar los empleados junto con el nombre de la sede, su turno y su cargo
SELECT 
    E.IdEmpleado,
    E.Nombre + ' ' + E.apellido_paterno AS Empleado,
    S.Nombre AS Sede,
    T.Nombre AS Turno,
    C.Nombre_cargo AS Cargo,
    E.Sueldo
FROM EMPLEADO E
INNER JOIN SEDE S ON E.IdSede = S.IdSede
INNER JOIN Turno T ON E.IdTurno = T.IdTurno
INNER JOIN Cargo C ON E.IdCargo = C.IdCargo;
GO

-- 2. Reporte de ventas mostrando el cliente, la fecha, y el monto total de la venta
SELECT 
    V.NroVenta,
    C.Nombre + ' ' + ISNULL(C.apellido_paterno, '') AS Cliente,
    V.FechaHora,
    SUM(DV.cantidad * DV.precio_venta) AS MontoTotal
FROM VENTA V
INNER JOIN CLIENTE C ON V.IdCliente = C.IdCliente
INNER JOIN DETALLE_VENTA DV ON V.NroVenta = DV.NroVenta
GROUP BY V.NroVenta, C.Nombre, C.apellido_paterno, V.FechaHora;
GO

-- 3. Listar las ventas que tuvieron envío, mostrando la empresa de transporte y el costo de envío
SELECT 
    V.NroVenta,
    C.Nombre + ' ' + C.apellido_paterno AS Cliente,
    V.FechaHora,
    ET.nombre AS EmpresaTransporte,
    E.fecha_entrega_estimado,
    E.costo_envio,
    E.estado_envio
FROM VENTA V
INNER JOIN ENVIA E ON V.NroVenta = E.NroVenta
INNER JOIN EMPRESA_TRANSPORTE ET ON E.IdTransporte = ET.IdTransporte
INNER JOIN CLIENTE C ON V.IdCliente = C.IdCliente;
GO

-- 4. Mostrar qué productos son suministrados por qué proveedores, junto con el precio unitario
SELECT 
    P.nombre AS Producto,
    PR.nombre AS Proveedor,
    S.precio_unitario,
    S.estado_suministro
FROM PRODUCTO P
INNER JOIN SUMINISTRA S ON P.IdProducto = S.IdProducto
INNER JOIN PROVEEDOR PR ON S.Id_Proveedor = PR.Id_Proveedor;
GO

-- 5. Listar las devoluciones realizadas, qué productos fueron devueltos, su cantidad y el motivo
SELECT 
    D.Nro_Devolucion,
    D.fechaHora,
    D.motivo_Devolucion,
    P.nombre AS Producto_Devuelto,
    C.cantidad_devuelta,
    C.precio_unitario_devuelto,
    E.Nombre AS Registrado_Por
FROM DEVOLUCION D
INNER JOIN CONTIENE C ON D.Nro_Devolucion = C.Nro_Devolucion
INNER JOIN PRODUCTO P ON C.IdProducto = P.IdProducto
INNER JOIN EMPLEADO E ON D.IdEmpleado = E.IdEmpleado;
GO

-- 6. Obtener los tipos de pago utilizados por las ventas y el monto total cobrado
SELECT 
    TP.NombrePago AS Tipo_Pago,
    COUNT(C.NroVenta) AS Cantidad_Transacciones,
    SUM(C.MontoPago) AS Total_Cobrado
FROM COBRA C
INNER JOIN TIPO_PAGO TP ON C.IdTipoPago = TP.IdTipoPago
GROUP BY TP.NombrePago;
GO

-- 7. Consultar detalles específicos para productos de tipo Café en Grano (con su origen)
SELECT 
    P.nombre AS Nombre_Producto,
    CG.tipo_grano,
    CG.proceso,
    O.Pais,
    O.Region
FROM PRODUCTO P
INNER JOIN CAFE_GRANO CG ON P.IdProducto = CG.IdProducto
INNER JOIN PROVIENE PR ON P.IdProducto = PR.IdProducto
INNER JOIN ORIGEN O ON PR.IdOrigen = O.IdOrigen;
GO
