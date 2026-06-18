-- ============================================================
-- PASO 6 — IMPLEMENTACIÓN DML (Data Manipulation Language)
-- Sistema: Café de Barrio POS
-- Motor:   Microsoft SQL Server (T-SQL)
-- Alumnos: Pablo Joel Castillo Flores, Justhin Christofher Huisa Valle, Jeremy Geraldo Armas Camones, Geradth Humberto Gaitan Gonzales, Allison Isabel Cordova Diaz
-- Fecha:   2026-06-15
-- Descripción: Script DML para la inserción de datos iniciales.
--              Puebla las tablas creadas en el DDL con datos de
--              prueba representativos del negocio (≥ 15 registros por tabla principal).
-- NOTA: Ejecutar EXCLUSIVAMENTE después de haber ejecutado con éxito PASO_6_DDL.sql.
-- ============================================================

USE CafeDeBarrioBD;
GO

-- ============================================================
-- NIVEL 0 — Sin dependencias externas
-- Se insertan primero porque otras tablas dependerán de estos registros.
-- ============================================================

-- 1. CategoriaCafe (6 registros)
--    Categorías básicas de los productos del menú.
INSERT INTO CategoriaCafe (codigo, nombre, descripcion, activa) VALUES
('CAF', 'Cafés Especiales',  'Cafés de origen single estate y mezclas premium', 1),
('BEB', 'Bebidas Frías',     'Frappés, limonadas y tisanas heladas',             1),
('COM', 'Comida',            'Snacks, postres y sándwiches artesanales',          1),
('TIS', 'Tisanas',           'Infusiones herbales y tés de especialidad',         1),
('MER', 'Merchandise',       'Granos, equipos y accesorios de barista',           1),
('PRO', 'Promociones',       'Combos y paquetes especiales de temporada',         1);
GO

-- 2. TipoCliente (3 registros)
--    Perfiles de clientes para aplicar promociones o fidelización.
INSERT INTO TipoCliente (nombre, descripcion) VALUES
('Regular',   'Cliente ocasional sin beneficios acumulados'),
('Frecuente', 'Cliente con historial de compras repetidas; accede a promociones'),
('Mayorista', 'Empresa o revendedor con precios diferenciados y crédito');
GO

-- 3. Sede (3 registros)
--    Sucursales físicas del café.
INSERT INTO Sede (nombre, direccion, distrito, ciudad, telefono, es_principal, activa, fecha_apertura) VALUES
('Café de Barrio Miraflores', 'Av. Principal 123',  'Miraflores', 'Lima', '01-234-5678', 1, 1, '2026-01-01'),
('Café de Barrio San Isidro', 'Av. Rivera Navarrete 456', 'San Isidro', 'Lima', '01-234-5679', 0, 1, '2026-03-01'),
('Café de Barrio Barranco',   'Jr. Lima 789',       'Barranco',   'Lima', '01-234-5680', 0, 1, '2026-05-01');
GO

-- 4. MetodoPago (6 registros)
--    Métodos configurados en la caja registradora.
INSERT INTO MetodoPago (nombre, activo, EsEfectivo) VALUES
('Efectivo',          1, 1),
('Tarjeta Débito',    1, 0),
('Tarjeta Crédito',   1, 0),
('Yape',              1, 0),
('Plin',              1, 0),
('Transferencia',     1, 0);
GO

-- 5. OpcionEnvio (4 registros)
--    Opciones logísticas de entrega con tarifas asociadas.
INSERT INTO OpcionEnvio (nombre, descripcion, tarifa, activa) VALUES
('Recojo en Tienda',   'El cliente retira en la sucursal sin costo adicional', 0.00, 1),
('Delivery Express',   'Entrega en menos de 30 minutos dentro del distrito',   5.00, 1),
('Delivery Estándar',  'Entrega en 1-2 horas en Lima Metropolitana',           3.50, 1),
('Servicio a Mesa',    'Atención en el local con asignación de mesa',          0.00, 1);
GO

-- ============================================================
-- NIVEL 1 — Dependen de Nivel 0
-- ============================================================

-- 6. ConfiguracionNegocio (3 registros, uno por sede)
--    TasaIGV=0.1600 (16%) + TasaIPM=0.0200 (2%) = 18% total (Impuestos de Perú)
INSERT INTO ConfiguracionNegocio (SedeId, TasaIGV, TasaIPM, FechaVigencia, Activo) VALUES
(1, 0.1600, 0.0200, '2026-01-01T00:00:00', 1),
(2, 0.1600, 0.0200, '2026-03-01T00:00:00', 1),
(3, 0.1600, 0.0200, '2026-05-01T00:00:00', 1);
GO

-- 7. Cliente (16 registros: 1 mostrador genérico + 15 clientes reales)
--    El ID '1' se usa para ventas rápidas que no requieren identificar al comprador.
INSERT INTO Cliente (tipo_cliente_id, nombre, apellido, email, codigo_cliente, tipo_documento, numero_documento, telefono, distrito, ciudad, fecha_registro, activo) VALUES
(1, 'Mostrador',  '',           'mostrador@cafedebarrio.local', NULL,  NULL,  NULL,         NULL,          'Miraflores', 'Lima', '2026-01-01', 1),
(1, 'Ana',        'García',     'ana.garcia@gmail.com',         'C001','DNI', '12345678',   '987-654-321', 'Miraflores', 'Lima', '2026-01-15', 1),
(1, 'Luis',       'Mamani',     'luis.mamani@hotmail.com',      'C002','DNI', '23456789',   '987-654-322', 'San Isidro', 'Lima', '2026-01-20', 1),
(2, 'Rosa',       'Flores',     'rosa.flores@gmail.com',        'C003','DNI', '34567890',   '987-654-323', 'Surco',      'Lima', '2026-02-01', 1),
(1, 'Carlos',     'Huanca',     'carlos.huanca@gmail.com',      'C004','DNI', '45678901',   '987-654-324', 'La Molina',  'Lima', '2026-02-10', 1),
(2, 'María',      'López',      'maria.lopez@outlook.com',      'C005','DNI', '56789012',   '987-654-325', 'Barranco',   'Lima', '2026-02-14', 1),
(1, 'Jorge',      'Vargas',     'jorge.vargas@gmail.com',       'C006','DNI', '67890123',   '987-654-326', 'Miraflores', 'Lima', '2026-02-20', 1),
(2, 'Elena',      'Quispe',     'elena.quispe@gmail.com',       'C007','DNI', '78901234',   '987-654-327', 'San Isidro', 'Lima', '2026-03-01', 1),
(3, 'Pedro',      'Ramos',      'pedro.ramos@gmail.com',        'C008','RUC', '20123456789','987-654-328', 'San Borja',  'Lima', '2026-03-05', 1),
(1, 'Sofía',      'Cruz',       'sofia.cruz@gmail.com',         'C009','DNI', '89012345',   '987-654-329', 'Lince',      'Lima', '2026-03-10', 1),
(2, 'Miguel',     'Torres',     'miguel.torres@gmail.com',      'C010','DNI', '90123456',   '987-654-330', 'Miraflores', 'Lima', '2026-03-15', 1),
(1, 'Lucia',      'Mendoza',    'lucia.mendoza@outlook.com',    'C011','DNI', '01234567',   '987-654-331', 'Barranco',   'Lima', '2026-03-20', 1),
(3, 'David',      'Paredes',    'david.paredes@gmail.com',      'C012','RUC', '20234567891','987-654-332', 'San Isidro', 'Lima', '2026-04-01', 1),
(1, 'Patricia',   'Soto',       'patricia.soto@gmail.com',      'C013','DNI', '13579246',   '987-654-333', 'Chorrillos', 'Lima', '2026-04-10', 1),
(2, 'Andrés',     'Salas',      'andres.salas@gmail.com',       'C014','DNI', '24680135',   '987-654-334', 'Miraflores', 'Lima', '2026-04-15', 1),
(1, 'Verónica',   'Díaz',       'veronica.diaz@gmail.com',      'C015','DNI', '35791246',   '987-654-335', 'San Isidro', 'Lima', '2026-05-01', 1);
GO

-- 8. Producto (16 registros — precios IGV-inclusivos)
--    Se asocian con categoria_id definido previamente en CategoriaCafe.
INSERT INTO Producto (categoria_id, nombre, descripcion, costo, precio, cantidad_disponible, stock_minimo, unidad_medida, seguimiento_inventario, es_mayorista, activo, created_at) VALUES
-- Bebidas Frías (categoria_id=2)
(2, 'Espresso Doble Clásico',    'Espresso intenso de doble carga, concentrado y aromático.',   3.50,  7.00, 80, 20, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Latte Vainilla de Barrio',  'Café latte con jarabe artesanal de vainilla.',                4.00,  9.50, 60, 15, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Capuccino Canela',          'Espresso con leche espumosa y toque de canela.',              3.80,  8.50, 70, 15, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Mocca Chocolate Amargo',    'Espresso, leche vaporizada y salsa de chocolate 70%.',        4.20, 10.00, 50, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Americano de la Casa',      'Café americano suave de tueste medio, ideal para el día.',    2.80,  6.50, 90, 25, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Cold Brew Cítrico',         'Cold brew de 12 horas con notas de naranja y bergamota.',     4.50, 11.00, 40, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Frappe Caramelo Salado',    'Café frappé blended con jarabe de caramelo salado.',          4.70, 11.50, 35, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Limonada con Hierbabuena',  'Limonada fresca exprimida con hierbabuena y azúcar de caña.', 2.50,  6.00, 80, 20, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(2, 'Té Helado Durazno',         'Té negro frío infusionado sabor durazno natural.',            2.40,  6.00, 60, 15, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
-- Cafés Especiales (categoria_id=1)
(1, 'Latte Avellana Rock',       'Latte con jarabe de avellana tostada y espuma sedosa.',       4.30, 10.50, 45, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(1, 'Café Nitro Barril',         'Café frío infusionado con nitrógeno; cremoso sin leche.',     5.20, 12.50, 30,  8, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(1, 'Affogato de Vainilla',      'Espresso caliente sobre helado artesanal de vainilla.',       4.80, 11.00, 35, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
-- Comida (categoria_id=3)
(3, 'Brownie Chocolate Intenso', 'Brownie casero de chocolate amargo, húmedo y denso.',         2.80,  7.00, 50, 15, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(3, 'Cheesecake de Maracuyá',    'Porción de cheesecake con cobertura de maracuyá natural.',    3.80,  9.50, 30, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(3, 'Sándwich de Pollo BBQ',     'Pan artesanal con pollo desmenuzado, salsa BBQ y lechuga.',   4.00, 10.00, 40, 10, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00'),
(3, 'Croissant de Mantequilla',  'Croissant hojaldrado horneado en el día, dorado y crujiente.',2.20,  5.50, 60, 15, 'Unidades', 1, 0, 1, '2026-01-01T08:00:00');
GO

-- 9. Operador (15 registros)
--    Empleados con acceso al sistema.
--    PinHash: en producción se usa Argon2; aquí se indica el hash correspondiente a un PIN estándar de prueba (ej. '123456')
INSERT INTO Operador (SedeId, Nombre, PinHash, Activo, Eliminado, FailedPinAttempts, IsLockedOut, CreatedAt) VALUES
(1, 'María Condori',    '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_01', 1, 0, 0, 0, '2026-01-01T08:00:00'),
(1, 'Juan Quispe',      '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_02', 1, 0, 0, 0, '2026-01-01T08:00:00'),
(1, 'Rosa Mamani',      '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_03', 1, 0, 0, 0, '2026-01-01T08:00:00'),
(1, 'Carlos Huanca',    '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_04', 1, 0, 0, 0, '2026-01-01T08:00:00'),
(1, 'Ana Torres',       '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_05', 1, 0, 0, 0, '2026-01-01T08:00:00'),
(2, 'Pedro Vargas',     '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_06', 1, 0, 0, 0, '2026-03-01T08:00:00'),
(2, 'Lucia Flores',     '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_07', 1, 0, 0, 0, '2026-03-01T08:00:00'),
(2, 'Miguel Ramos',     '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_08', 1, 0, 0, 0, '2026-03-01T08:00:00'),
(3, 'Sofía Cruz',       '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_09', 1, 0, 0, 0, '2026-05-01T08:00:00'),
(3, 'Andrés Salas',     '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_10', 1, 0, 0, 0, '2026-05-01T08:00:00'),
(1, 'Verónica Díaz',    '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_11', 1, 0, 0, 0, '2026-01-10T08:00:00'),
(1, 'Jorge López',      '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_12', 1, 0, 0, 0, '2026-01-15T08:00:00'),
(2, 'Elena Paredes',    '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_13', 1, 0, 0, 0, '2026-03-10T08:00:00'),
(3, 'David Soto',       '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_14', 1, 0, 0, 0, '2026-05-10T08:00:00'),
(1, 'Patricia Medina',  '$argon2id$v=19$m=65536,t=3$PLACEHOLDER_HASH_15', 1, 0, 0, 0, '2026-02-01T08:00:00');
GO

-- ============================================================
-- NIVEL 2 — Turno
-- ============================================================

-- 10. Turno (15 registros: 14 cerrados + 1 abierto)
--     Un turno asocia un Operador a una Sede con montos de cuadre de caja.
INSERT INTO Turno (SedeId, OperadorId, FechaApertura, FechaCierre, MontoApertura, MontoEfectivoCierto, TotalEfectivoSistema, TotalVentasEfectivo, TotalAnulacionesEfectivo, TotalMovimientosEntrada, TotalMovimientosSalida, SaldoEsperado, Diferencia, Estado, CreatedAt) VALUES
(1, 1, '2026-05-17T08:00:00', '2026-05-17T16:00:00', 100.00, 487.50, 487.50, 387.50, 0.00, 0.00, 0.00, 487.50, 0.00, 'Cerrado', '2026-05-17T08:00:00'),
(1, 2, '2026-05-18T08:00:00', '2026-05-18T16:00:00', 100.00, 520.00, 520.00, 420.00, 6.50, 0.00, 0.00, 513.50, 6.50, 'Cerrado', '2026-05-18T08:00:00'),
(1, 3, '2026-05-19T08:00:00', '2026-05-19T16:00:00', 100.00, 345.00, 345.00, 245.00, 0.00, 0.00, 0.00, 345.00, 0.00, 'Cerrado', '2026-05-19T08:00:00'),
(1, 4, '2026-05-20T08:00:00', '2026-05-20T16:00:00', 100.00, 610.00, 610.00, 510.00, 14.00, 0.00, 50.00, 546.00, 64.00, 'Cerrado', '2026-05-20T08:00:00'),
(1, 5, '2026-05-21T08:00:00', '2026-05-21T16:00:00', 100.00, 298.00, 298.00, 198.00, 0.00, 0.00, 0.00, 298.00, 0.00, 'Cerrado', '2026-05-21T08:00:00'),
(2, 6, '2026-05-22T09:00:00', '2026-05-22T17:00:00', 150.00, 430.00, 430.00, 280.00, 0.00, 0.00, 0.00, 430.00, 0.00, 'Cerrado', '2026-05-22T09:00:00'),
(2, 7, '2026-05-23T09:00:00', '2026-05-23T17:00:00', 150.00, 390.00, 390.00, 240.00, 9.50, 0.00, 0.00, 380.50, 9.50, 'Cerrado', '2026-05-23T09:00:00'),
(1, 1, '2026-05-24T08:00:00', '2026-05-24T16:00:00', 100.00, 545.00, 545.00, 445.00, 0.00, 0.00, 0.00, 545.00, 0.00, 'Cerrado', '2026-05-24T08:00:00'),
(1, 2, '2026-05-25T08:00:00', '2026-05-25T16:00:00', 100.00, 312.50, 312.50, 212.50, 8.50, 0.00, 0.00, 304.00, 8.50, 'Cerrado', '2026-05-25T08:00:00'),
(3, 9, '2026-05-26T10:00:00', '2026-05-26T18:00:00', 200.00, 580.00, 580.00, 380.00, 0.00, 0.00, 0.00, 580.00, 0.00, 'Cerrado', '2026-05-26T10:00:00'),
(1, 3, '2026-06-10T08:00:00', '2026-06-10T16:00:00', 100.00, 467.00, 467.00, 367.00, 11.00, 0.00, 0.00, 456.00, 11.00, 'Cerrado', '2026-06-10T08:00:00'),
(1, 4, '2026-06-11T08:00:00', '2026-06-11T16:00:00', 100.00, 534.50, 534.50, 434.50, 0.00, 0.00, 0.00, 534.50, 0.00, 'Cerrado', '2026-06-11T08:00:00'),
(2, 8, '2026-06-12T09:00:00', '2026-06-12T17:00:00', 150.00, 720.00, 720.00, 570.00, 22.00, 0.00, 0.00, 698.00, 22.00, 'Cerrado', '2026-06-12T09:00:00'),
(1, 5, '2026-06-13T08:00:00', '2026-06-13T16:00:00', 100.00, 398.50, 398.50, 298.50, 0.00, 0.00, 0.00, 398.50, 0.00, 'Cerrado', '2026-06-13T08:00:00'),
-- Turno 15: Actualmente abierto (sin fecha ni cálculos de cierre)
(1, 1, '2026-06-15T08:00:00', NULL,                  100.00, NULL,   NULL,   0.00,   0.00, 0.00, 0.00, 100.00, NULL, 'Abierto', '2026-06-15T08:00:00');
GO

-- ============================================================
-- NIVEL 3 — MovimientoCaja
-- ============================================================

-- 11. MovimientoCaja (15 registros)
--     Registra los ajustes en la gaveta de efectivo independientes a las ventas.
--     Entradas: apertura, refuerzo, pagos de terceros a caja. Salidas: vuelto, gastos operativos.
INSERT INTO MovimientoCaja (TurnoId, TipoMovimiento, Motivo, Monto, FechaHora, CreatedAt) VALUES
(1, 'Salida',  'Compra de insumos cafetería',            50.00, '2026-05-17T10:30:00', '2026-05-17T10:30:00'),
(2, 'Salida',  'Vuelto de billete S/100',                 6.50, '2026-05-18T11:00:00', '2026-05-18T11:00:00'),
(3, 'Entrada', 'Refuerzo de caja autorizado por gerente', 100.00,'2026-05-19T09:00:00', '2026-05-19T09:00:00'),
(4, 'Salida',  'Pago a proveedor de granos',              50.00, '2026-05-20T12:00:00', '2026-05-20T12:00:00'),
(4, 'Salida',  'Pago de servicio técnico balanza',        14.00, '2026-05-20T14:00:00', '2026-05-20T14:00:00'),
(5, 'Salida',  'Compra de servilletas y vasos',           20.00, '2026-05-21T10:00:00', '2026-05-21T10:00:00'),
(6, 'Entrada', 'Fondo inicial adicional para fin de semana', 50.00,'2026-05-22T09:05:00','2026-05-22T09:05:00'),
(7, 'Salida',  'Pago de limpieza mensual',                25.00, '2026-05-23T15:00:00', '2026-05-23T15:00:00'),
(8, 'Salida',  'Compra insumos leche y crema',            45.00, '2026-05-24T11:00:00', '2026-05-24T11:00:00'),
(9, 'Entrada', 'Cobro por catering externo',              80.00, '2026-05-25T13:00:00', '2026-05-25T13:00:00'),
(10,'Salida',  'Pago de alquiler parcial',               200.00, '2026-05-26T17:00:00', '2026-05-26T17:00:00'),
(11,'Salida',  'Compra de azúcar y endulzantes',          15.00, '2026-06-10T10:00:00', '2026-06-10T10:00:00'),
(12,'Entrada', 'Refuerzo de efectivo autorizado',         100.00, '2026-06-11T09:00:00','2026-06-11T09:00:00'),
(13,'Salida',  'Reparación de cafetera',                  60.00, '2026-06-12T14:00:00', '2026-06-12T14:00:00'),
(14,'Salida',  'Compra de empaques delivery',             18.00, '2026-06-13T10:30:00', '2026-06-13T10:30:00');
GO

-- ============================================================
-- NIVEL 4 — Transaccion (modelo IGV extractivo)
-- Representa la cabecera de las boletas/facturas generadas.
-- Fórmulas utilizadas:
--   subtotal  = ROUND(total / 1.18, 2)  -- Monto sin IGV
--   impuesto  = ROUND(total - subtotal, 2) -- Monto del IGV
-- ============================================================

-- 12. Transaccion (20 registros)
INSERT INTO Transaccion (cliente_id, sede_id, metodo_pago_id, turno_id, operador_id, es_mayorista, canal, fecha, subtotal, impuesto, recargo_propina, costo_envio, total, SunatEstado, SunatIntentos, created_at) VALUES
-- T1: Americano x1 = 6.50 → sub=5.51, imp=0.99
(2,  1, 1, 1,  1, 0, 'POS', '2026-05-17T09:15:00', 5.51, 0.99, 0.00, 0.00, 6.50,  'Aceptado', 1, '2026-05-17T09:15:00'),
-- T2: Latte Vainilla x1 = 9.50 → sub=8.05, imp=1.45
(3,  1, 4, 1,  1, 0, 'POS', '2026-05-17T10:00:00', 8.05, 1.45, 0.00, 0.00, 9.50,  'Aceptado', 1, '2026-05-17T10:00:00'),
-- T3: Espresso x2 = 14.00 → sub=11.86, imp=2.14
(1,  1, 1, 2,  2, 0, 'POS', '2026-05-18T09:30:00',11.86, 2.14, 0.00, 0.00,14.00,  'Aceptado', 1, '2026-05-18T09:30:00'),
-- T4: Brownie x1 = 7.00 → sub=5.93, imp=1.07
(4,  1, 1, 2,  2, 0, 'POS', '2026-05-18T11:45:00', 5.93, 1.07, 0.00, 0.00, 7.00,  'Aceptado', 1, '2026-05-18T11:45:00'),
-- T5: Capuccino x1 = 8.50 → sub=7.20, imp=1.30
(5,  1, 4, 3,  3, 0, 'POS', '2026-05-19T08:45:00', 7.20, 1.30, 0.00, 0.00, 8.50,  'Aceptado', 1, '2026-05-19T08:45:00'),
-- T6: Cold Brew x1 = 11.00 → sub=9.32, imp=1.68
(6,  1, 1, 3,  3, 0, 'POS', '2026-05-19T13:00:00', 9.32, 1.68, 0.00, 0.00,11.00,  'Aceptado', 1, '2026-05-19T13:00:00'),
-- T7: Frappe Caramelo x1 = 11.50 → sub=9.75, imp=1.75
(7,  1, 4, 4,  4, 0, 'POS', '2026-05-20T10:00:00', 9.75, 1.75, 0.00, 0.00,11.50,  'Aceptado', 1, '2026-05-20T10:00:00'),
-- T8: Limonada x2 = 12.00 → sub=10.17, imp=1.83
(8,  1, 1, 4,  4, 0, 'POS', '2026-05-20T12:30:00',10.17, 1.83, 0.00, 0.00,12.00,  'Aceptado', 1, '2026-05-20T12:30:00'),
-- T9: Cheesecake x1 = 9.50 → sub=8.05, imp=1.45
(9,  1, 1, 5,  5, 0, 'POS', '2026-05-21T09:00:00', 8.05, 1.45, 0.00, 0.00, 9.50,  'Aceptado', 1, '2026-05-21T09:00:00'),
-- T10: Sándwich x1 = 10.00 → sub=8.47, imp=1.53
(10, 2, 4, 6,  6, 0, 'POS', '2026-05-22T10:30:00', 8.47, 1.53, 0.00, 0.00,10.00,  'Aceptado', 1, '2026-05-22T10:30:00'),
-- T11: Latte Avellana x1 = 10.50 → sub=8.90, imp=1.60
(11, 2, 1, 6,  6, 0, 'POS', '2026-05-22T11:00:00', 8.90, 1.60, 0.00, 0.00,10.50,  'Aceptado', 1, '2026-05-22T11:00:00'),
-- T12: Café Nitro x1 = 12.50 → sub=10.59, imp=1.91
(12, 2, 4, 7,  7, 0, 'POS', '2026-05-23T14:00:00',10.59, 1.91, 0.00, 0.00,12.50,  'Aceptado', 1, '2026-05-23T14:00:00'),
-- T13: Affogato x2 = 22.00 → sub=18.64, imp=3.36
(13, 1, 1, 8,  1, 0, 'POS', '2026-05-24T09:15:00',18.64, 3.36, 0.00, 0.00,22.00,  'Aceptado', 1, '2026-05-24T09:15:00'),
-- T14: Espresso x1 + Brownie x1 = 14.00 → sub=11.86, imp=2.14
(14, 1, 4, 8,  1, 0, 'POS', '2026-05-24T11:30:00',11.86, 2.14, 0.00, 0.00,14.00,  'Aceptado', 1, '2026-05-24T11:30:00'),
-- T15: Té Helado x2 = 12.00 → sub=10.17, imp=1.83
(15, 1, 1, 9,  2, 0, 'POS', '2026-05-25T10:00:00',10.17, 1.83, 0.00, 0.00,12.00,  'Aceptado', 1, '2026-05-25T10:00:00'),
-- T16: Mocca x1 = 10.00 → sub=8.47, imp=1.53
(2,  3, 4,10,  9, 0, 'POS', '2026-05-26T11:00:00', 8.47, 1.53, 0.00, 0.00,10.00,  'Aceptado', 1, '2026-05-26T11:00:00'),
-- T17: Croissant x2 = 11.00 → sub=9.32, imp=1.68
(3,  1, 1,11,  3, 0, 'POS', '2026-06-10T09:00:00', 9.32, 1.68, 0.00, 0.00,11.00,  'Aceptado', 1, '2026-06-10T09:00:00'),
-- T18: Latte Vainilla x2 = 19.00 → sub=16.10, imp=2.90
(4,  1, 4,12,  4, 0, 'POS', '2026-06-11T12:00:00',16.10, 2.90, 0.00, 0.00,19.00,  'Aceptado', 1, '2026-06-11T12:00:00'),
-- T19: Cold Brew x2 = 22.00 → sub=18.64, imp=3.36
(5,  2, 1,13,  8, 0, 'POS', '2026-06-12T10:30:00',18.64, 3.36, 0.00, 0.00,22.00,  'Aceptado', 1, '2026-06-12T10:30:00'),
-- T20: Americano x3 = 19.50 → sub=16.53, imp=2.97
(6,  1, 4,14,  5, 0, 'POS', '2026-06-13T09:45:00',16.53, 2.97, 0.00, 0.00,19.50,  'Aceptado', 1, '2026-06-13T09:45:00');
GO

-- ============================================================
-- NIVEL 5 — DetalleTransaccion
-- Detalle línea a línea de los productos vendidos en cada transacción.
-- subtotal_linea = ROUND((precio_unitario / 1.18) * cantidad, 2)
-- ============================================================

-- 13. DetalleTransaccion (22 líneas para 20 transacciones)
INSERT INTO DetalleTransaccion (transaccion_id, producto_id, cantidad, precio_unitario, subtotal_linea, CreatedAt) VALUES
-- T1: Americano x1  → 6.50/1.18=5.5084→5.51
(1,  5, 1,  6.50,  5.51, '2026-05-17T09:15:00'),
-- T2: Latte Vainilla x1 → 9.50/1.18=8.0508→8.05
(2,  2, 1,  9.50,  8.05, '2026-05-17T10:00:00'),
-- T3: Espresso x2 → (7.00/1.18)*2=11.864→11.86
(3,  1, 2,  7.00, 11.86, '2026-05-18T09:30:00'),
-- T4: Brownie x1 → 7.00/1.18=5.932→5.93
(4, 13, 1,  7.00,  5.93, '2026-05-18T11:45:00'),
-- T5: Capuccino x1 → 8.50/1.18=7.2033→7.20
(5,  3, 1,  8.50,  7.20, '2026-05-19T08:45:00'),
-- T6: Cold Brew x1 → 11.00/1.18=9.3220→9.32
(6,  6, 1, 11.00,  9.32, '2026-05-19T13:00:00'),
-- T7: Frappe x1 → 11.50/1.18=9.7457→9.75
(7,  7, 1, 11.50,  9.75, '2026-05-20T10:00:00'),
-- T8: Limonada x2 → (6.00/1.18)*2=10.1694→10.17
(8,  8, 2,  6.00, 10.17, '2026-05-20T12:30:00'),
-- T9: Cheesecake x1 → 9.50/1.18=8.0508→8.05
(9, 14, 1,  9.50,  8.05, '2026-05-21T09:00:00'),
-- T10: Sándwich x1 → 10.00/1.18=8.4745→8.47
(10,15, 1, 10.00,  8.47, '2026-05-22T10:30:00'),
-- T11: Latte Avellana x1 → 10.50/1.18=8.8983→8.90
(11,10, 1, 10.50,  8.90, '2026-05-22T11:00:00'),
-- T12: Café Nitro x1 → 12.50/1.18=10.5932→10.59
(12,11, 1, 12.50, 10.59, '2026-05-23T14:00:00'),
-- T13: Affogato x2 → (11.00/1.18)*2=18.6440→18.64
(13,12, 2, 11.00, 18.64, '2026-05-24T09:15:00'),
-- T14: Espresso x1 + Brownie x1 (2 líneas de detalle para 1 boleta)
(14, 1, 1,  7.00,  5.93, '2026-05-24T11:30:00'),
(14,13, 1,  7.00,  5.93, '2026-05-24T11:30:00'),
-- T15: Té Helado x2 → (6.00/1.18)*2=10.1694→10.17
(15, 9, 2,  6.00, 10.17, '2026-05-25T10:00:00'),
-- T16: Mocca x1 → 10.00/1.18=8.4745→8.47
(16, 4, 1, 10.00,  8.47, '2026-05-26T11:00:00'),
-- T17: Croissant x2 → (5.50/1.18)*2=9.3220→9.32
(17,16, 2,  5.50,  9.32, '2026-06-10T09:00:00'),
-- T18: Latte Vainilla x2 → (9.50/1.18)*2=16.1016→16.10
(18, 2, 2,  9.50, 16.10, '2026-06-11T12:00:00'),
-- T19: Cold Brew x2 → (11.00/1.18)*2=18.6440→18.64
(19, 6, 2, 11.00, 18.64, '2026-06-12T10:30:00'),
-- T20: Americano x3 → (6.50/1.18)*3=16.5254→16.53
(20, 5, 3,  6.50, 16.53, '2026-06-13T09:45:00');
GO

-- ============================================================
-- NIVEL 6 — Anulacion (referencia 1:1 con Transaccion)
-- Registra las devoluciones y cancelaciones de ventas previas.
-- ============================================================

-- 14. Anulacion (10 registros: anulaciones de T1 a T10)
INSERT INTO Anulacion (TransaccionId, TipoAnulacion, Motivo, MontoOriginal, MontoDevuelto, MetodoDevolucion, OperadorSolicitanteId, AutorizadorId, FechaHora, ImpactoInventario, CreatedAt) VALUES
(1,  'DevolucionTotal',   'Error en pedido: cliente pidió con leche y es intolerante',    6.50,  6.50, 'Efectivo', 2, 1, '2026-05-17T09:45:00', 1, '2026-05-17T09:45:00'),
(2,  'DevolucionTotal',   'Producto mal preparado: temperatura incorrecta',                9.50,  9.50, 'Efectivo', 3, 1, '2026-05-17T10:30:00', 1, '2026-05-17T10:30:00'),
(3,  'DevolucionTotal',   'Doble cobro detectado en terminal',                            14.00, 14.00, 'Efectivo', 1, 1, '2026-05-18T10:00:00', 1, '2026-05-18T10:00:00'),
(4,  'DevolucionTotal',   'Brownie vencido: fecha de producción superada',                 7.00,  7.00, 'Efectivo', 4, 2, '2026-05-18T12:15:00', 1, '2026-05-18T12:15:00'),
(5,  'DevolucionTotal',   'Pedido incorrecto: el cliente solicitó otra variedad',          8.50,  8.50, 'Yape',     3, 2, '2026-05-19T09:15:00', 1, '2026-05-19T09:15:00'),
(6,  'Cancelacion',       'Equipo defectuoso: cold brew no listo al momento del cobro',   11.00, 11.00, 'Efectivo', 5, 1, '2026-05-19T13:30:00', 0, '2026-05-19T13:30:00'),
(7,  'DevolucionTotal',   'Alergia declarada post-compra: cliente alérgico a caramelo',   11.50, 11.50, 'Efectivo', 4, 2, '2026-05-20T10:30:00', 1, '2026-05-20T10:30:00'),
(8,  'DevolucionParcial', 'Solo una limonada servida de dos solicitadas',                 12.00,  6.00, 'Efectivo', 2, 1, '2026-05-20T13:00:00', 1, '2026-05-20T13:00:00'),
(9,  'DevolucionTotal',   'Error de sistema: transacción duplicada',                       9.50,  9.50, 'Yape',     5, 1, '2026-05-21T09:30:00', 0, '2026-05-21T09:30:00'),
(10, 'DevolucionParcial', 'Sándwich sin el adicional pagado; descuento aplicado',         10.00,  5.00, 'Efectivo', 6, 1, '2026-05-22T11:00:00', 1, '2026-05-22T11:00:00');
GO

-- ============================================================
-- FIN DEL SCRIPT DML
-- Registros insertados:
--   CategoriaCafe:       6
--   TipoCliente:         3
--   Sede:                3
--   MetodoPago:          6
--   OpcionEnvio:         4
--   ConfiguracionNegocio:3
--   Cliente:            16
--   Producto:           16
--   Operador:           15
--   Turno:              15
--   MovimientoCaja:     15
--   Transaccion:        20
--   DetalleTransaccion: 22
--   Anulacion:          10
-- Total registros insertados: 154
-- ============================================================
