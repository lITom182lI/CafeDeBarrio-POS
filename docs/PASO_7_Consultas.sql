-- ============================================================
-- PASO 7 — CONSULTAS DE GESTIÓN
-- Sistema: Café de Barrio POS
-- Motor:   Microsoft SQL Server (T-SQL)
-- Alumnos: Pablo Joel Castillo Flores, Justhin Christofher Huisa Valle, Jeremy Geraldo Armas Camones, Geradth Humberto Gaitan Gonzales, Allison Isabel Cordova Diaz
-- Fecha:   2026-06-15
-- Descripción: 15 consultas de gestión que demuestran el valor
--              analítico del sistema. Incluye JOINs múltiples,
--              subconsultas, agregaciones y expresiones CASE.
-- Prerrequisito: Haber ejecutado PASO_6_DDL.sql y PASO_6_DML.sql
-- ============================================================

USE CafeDeBarrioBD;
GO

-- ============================================================
-- CONSULTA 1 — Ventas totales por día en un rango de fechas
-- Tipo: Agregación + GROUP BY + rango de fechas
-- Uso gerencial: monitoreo de recaudación diaria
-- ============================================================
SELECT
    CAST(t.fecha AS DATE)       AS Dia,
    COUNT(t.transaccion_id)     AS NumeroVentas,
    SUM(t.subtotal)             AS BaseImponible,
    SUM(t.impuesto)             AS IGV_IPM,
    SUM(t.total)                AS TotalRecaudado
FROM Transaccion t
WHERE t.fecha >= '2026-05-01'
  AND t.fecha <  '2026-07-01'
GROUP BY CAST(t.fecha AS DATE)
ORDER BY Dia;
GO

-- ============================================================
-- CONSULTA 2 — Top 10 productos más vendidos (por unidades)
-- Tipo: JOIN + GROUP BY + ORDER BY + TOP
-- Uso gerencial: decisiones de stock y carta del menú
-- ============================================================
SELECT TOP 10
    p.nombre                    AS Producto,
    cc.nombre                   AS Categoria,
    SUM(dt.cantidad)            AS UnidadesVendidas,
    SUM(dt.cantidad * dt.precio_unitario) AS IngresosBrutos
FROM DetalleTransaccion dt
INNER JOIN Producto      p  ON dt.producto_id    = p.producto_id
INNER JOIN CategoriaCafe cc ON p.categoria_id    = cc.categoria_id
GROUP BY p.producto_id, p.nombre, cc.nombre
ORDER BY UnidadesVendidas DESC;
GO

-- ============================================================
-- CONSULTA 3 — Ventas por método de pago
-- Tipo: JOIN + GROUP BY + porcentaje calculado
-- Uso gerencial: proyección de flujo de efectivo vs digital
-- ============================================================
SELECT
    mp.nombre                   AS MetodoPago,
    mp.EsEfectivo               AS EsEfectivo,
    COUNT(t.transaccion_id)     AS NumeroTransacciones,
    SUM(t.total)                AS TotalRecaudado,
    ROUND(
        100.0 * SUM(t.total) / SUM(SUM(t.total)) OVER (),
    2)                          AS PorcentajeDelTotal
FROM Transaccion t
INNER JOIN MetodoPago mp ON t.metodo_pago_id = mp.metodo_pago_id
GROUP BY mp.metodo_pago_id, mp.nombre, mp.EsEfectivo
ORDER BY TotalRecaudado DESC;
GO

-- ============================================================
-- CONSULTA 4 — Ventas por franja horaria
-- Tipo: CASE + GROUP BY + conteo
-- Uso gerencial: dimensionamiento de personal por turno
-- ============================================================
SELECT
    CASE
        WHEN DATEPART(HOUR, t.fecha) BETWEEN  6 AND 11 THEN 'Mañana  (06-11h)'
        WHEN DATEPART(HOUR, t.fecha) BETWEEN 12 AND 15 THEN 'Mediodía (12-15h)'
        WHEN DATEPART(HOUR, t.fecha) BETWEEN 16 AND 19 THEN 'Tarde   (16-19h)'
        ELSE                                                 'Noche   (20-23h)'
    END                         AS FranjaHoraria,
    COUNT(t.transaccion_id)     AS NumeroVentas,
    SUM(t.total)                AS TotalRecaudado,
    ROUND(AVG(t.total), 2)      AS TicketPromedio
FROM Transaccion t
GROUP BY
    CASE
        WHEN DATEPART(HOUR, t.fecha) BETWEEN  6 AND 11 THEN 'Mañana  (06-11h)'
        WHEN DATEPART(HOUR, t.fecha) BETWEEN 12 AND 15 THEN 'Mediodía (12-15h)'
        WHEN DATEPART(HOUR, t.fecha) BETWEEN 16 AND 19 THEN 'Tarde   (16-19h)'
        ELSE                                                 'Noche   (20-23h)'
    END
ORDER BY TotalRecaudado DESC;
GO

-- ============================================================
-- CONSULTA 5 — Operadores con mayor volumen de ventas
-- Tipo: JOIN múltiple + GROUP BY + ORDER BY
-- Uso gerencial: evaluación de desempeño por cajero
-- ============================================================
SELECT
    o.Nombre                    AS Operador,
    s.nombre                    AS Sede,
    COUNT(t.transaccion_id)     AS NumeroVentas,
    SUM(t.total)                AS TotalVendido,
    ROUND(AVG(t.total), 2)      AS TicketPromedio
FROM Transaccion t
INNER JOIN Operador o ON t.operador_id = o.OperadorId
INNER JOIN Sede     s ON o.SedeId      = s.sede_id
GROUP BY o.OperadorId, o.Nombre, s.nombre
ORDER BY TotalVendido DESC;
GO

-- ============================================================
-- CONSULTA 6 — Productos con stock por debajo del mínimo
-- Tipo: WHERE simple + subconsulta correlacionada
-- Uso gerencial: alerta de reposición de inventario
-- ============================================================
SELECT
    p.nombre                    AS Producto,
    cc.nombre                   AS Categoria,
    p.cantidad_disponible       AS StockActual,
    p.stock_minimo              AS StockMinimo,
    p.stock_minimo - p.cantidad_disponible AS Deficit,
    -- Cuántas unidades se vendieron en los últimos 30 días
    (SELECT COALESCE(SUM(dt2.cantidad), 0)
     FROM DetalleTransaccion dt2
     INNER JOIN Transaccion t2 ON dt2.transaccion_id = t2.transaccion_id
     WHERE dt2.producto_id = p.producto_id
       AND t2.fecha >= DATEADD(DAY, -30, GETDATE())
    )                           AS VendidosUltimos30Dias
FROM Producto p
INNER JOIN CategoriaCafe cc ON p.categoria_id = cc.categoria_id
WHERE p.cantidad_disponible < p.stock_minimo
  AND p.activo = 1
ORDER BY Deficit DESC;
GO

-- ============================================================
-- CONSULTA 7 — Historial de anulaciones con detalle completo
-- Tipo: JOIN múltiple (4 tablas)
-- Uso gerencial: auditoría de reversiones y control de leakage
-- ============================================================
SELECT
    a.AnulacionId,
    t.fecha                     AS FechaVenta,
    a.FechaHora                 AS FechaAnulacion,
    a.TipoAnulacion,
    a.Motivo,
    t.total                     AS MontoOriginal,
    a.MontoDevuelto,
    a.MetodoDevolucion,
    sol.Nombre                  AS OperadorSolicitante,
    aut.Nombre                  AS Autorizador
FROM Anulacion a
INNER JOIN Transaccion t   ON a.TransaccionId         = t.transaccion_id
INNER JOIN Operador    sol ON a.OperadorSolicitanteId  = sol.OperadorId
INNER JOIN Operador    aut ON a.AutorizadorId          = aut.OperadorId
ORDER BY a.FechaHora DESC;
GO

-- ============================================================
-- CONSULTA 8 — Resumen de cierre de turno
-- Tipo: JOIN + SUM agrupado + cálculo de diferencia
-- Uso gerencial: cuadre de caja al final de cada turno
-- ============================================================
SELECT
    tr.TurnoId,
    o.Nombre                        AS Operador,
    s.nombre                        AS Sede,
    tr.FechaApertura,
    tr.FechaCierre,
    tr.MontoApertura,
    tr.TotalVentasEfectivo,
    tr.TotalAnulacionesEfectivo,
    tr.TotalMovimientosEntrada,
    tr.TotalMovimientosSalida,
    tr.SaldoEsperado,
    tr.MontoEfectivoCierto          AS EfectivoContado,
    COALESCE(tr.Diferencia, 0)      AS Diferencia,
    tr.Estado
FROM Turno    tr
INNER JOIN Operador o ON tr.OperadorId = o.OperadorId
INNER JOIN Sede     s ON tr.SedeId     = s.sede_id
ORDER BY tr.FechaApertura DESC;
GO

-- ============================================================
-- CONSULTA 9 — Clientes con mayor gasto acumulado
-- Tipo: JOIN + subconsulta + ORDER BY
-- Uso gerencial: identificar candidatos a programa de fidelidad
-- ============================================================
SELECT
    c.nombre + ' ' + c.apellido AS Cliente,
    tc.nombre                   AS TipoCliente,
    COUNT(t.transaccion_id)     AS NumeroCompras,
    SUM(t.total)                AS GastoTotal,
    ROUND(AVG(t.total), 2)      AS PromedioTicket,
    MAX(t.fecha)                AS UltimaCompra
FROM Transaccion t
INNER JOIN Cliente    c  ON t.cliente_id       = c.cliente_id
INNER JOIN TipoCliente tc ON c.tipo_cliente_id = tc.tipo_cliente_id
GROUP BY c.cliente_id, c.nombre, c.apellido, tc.nombre
ORDER BY GastoTotal DESC;
GO

-- ============================================================
-- CONSULTA 10 — Ticket promedio por operador y sede
-- Tipo: AVG + JOIN + GROUP BY
-- Uso gerencial: benchmark de productividad entre sucursales
-- ============================================================
SELECT
    s.nombre                    AS Sede,
    o.Nombre                    AS Operador,
    COUNT(t.transaccion_id)     AS CantidadVentas,
    ROUND(AVG(t.total), 2)      AS TicketPromedio,
    MIN(t.total)                AS VentaMinima,
    MAX(t.total)                AS VentaMaxima
FROM Transaccion t
INNER JOIN Operador o ON t.operador_id = o.OperadorId
INNER JOIN Sede     s ON o.SedeId      = s.sede_id
GROUP BY s.sede_id, s.nombre, o.OperadorId, o.Nombre
HAVING COUNT(t.transaccion_id) >= 1
ORDER BY Sede, TicketPromedio DESC;
GO

-- ============================================================
-- CONSULTA 11 — Productos sin ventas en los últimos 30 días
-- Tipo: LEFT JOIN + WHERE IS NULL (anti-join)
-- Uso gerencial: detectar ítems candidatos a ser retirados del menú
-- ============================================================
SELECT
    p.nombre                    AS Producto,
    cc.nombre                   AS Categoria,
    p.precio                    AS Precio,
    p.cantidad_disponible       AS StockActual,
    p.activo                    AS Activo
FROM Producto p
INNER JOIN CategoriaCafe cc ON p.categoria_id = cc.categoria_id
LEFT JOIN (
    SELECT DISTINCT dt.producto_id
    FROM DetalleTransaccion dt
    INNER JOIN Transaccion t ON dt.transaccion_id = t.transaccion_id
    WHERE t.fecha >= DATEADD(DAY, -30, GETDATE())
) AS vendidos ON p.producto_id = vendidos.producto_id
WHERE vendidos.producto_id IS NULL
  AND p.activo = 1
ORDER BY cc.nombre, p.nombre;
GO

-- ============================================================
-- CONSULTA 12 — Recaudación diaria: efectivo vs pago digital
-- Tipo: CASE + SUM condicional + GROUP BY
-- Uso gerencial: proyección de liquidez y depósitos bancarios
-- ============================================================
SELECT
    CAST(t.fecha AS DATE)                        AS Dia,
    SUM(CASE WHEN mp.EsEfectivo = 1
             THEN t.total ELSE 0 END)            AS TotalEfectivo,
    SUM(CASE WHEN mp.EsEfectivo = 0
             THEN t.total ELSE 0 END)            AS TotalDigital,
    SUM(t.total)                                 AS TotalGeneral,
    COUNT(CASE WHEN mp.EsEfectivo = 1
               THEN 1 END)                       AS TransaccionesEfectivo,
    COUNT(CASE WHEN mp.EsEfectivo = 0
               THEN 1 END)                       AS TransaccionesDigital
FROM Transaccion t
INNER JOIN MetodoPago mp ON t.metodo_pago_id = mp.metodo_pago_id
GROUP BY CAST(t.fecha AS DATE)
ORDER BY Dia;
GO

-- ============================================================
-- CONSULTA 13 — Movimientos de caja por turno
-- Tipo: JOIN + ORDER BY + agrupación por tipo
-- Uso gerencial: trazabilidad de cada centavo que entra/sale de caja
-- ============================================================
SELECT
    m.MovimientoCajaId,
    tr.TurnoId,
    o.Nombre                    AS Operador,
    s.nombre                    AS Sede,
    m.TipoMovimiento,
    m.Motivo,
    m.Monto,
    m.FechaHora,
    SUM(CASE WHEN m.TipoMovimiento = 'Entrada'
             THEN m.Monto ELSE 0 END)
        OVER (PARTITION BY m.TurnoId)  AS TotalEntradasTurno,
    SUM(CASE WHEN m.TipoMovimiento = 'Salida'
             THEN m.Monto ELSE 0 END)
        OVER (PARTITION BY m.TurnoId)  AS TotalSalidasTurno
FROM MovimientoCaja m
INNER JOIN Turno    tr ON m.TurnoId    = tr.TurnoId
INNER JOIN Operador o  ON tr.OperadorId = o.OperadorId
INNER JOIN Sede     s  ON tr.SedeId     = s.sede_id
ORDER BY m.TurnoId, m.FechaHora;
GO

-- ============================================================
-- CONSULTA 14 — Margen bruto por categoría
-- Tipo: JOIN + expresión aritmética + GROUP BY
-- Uso gerencial: rentabilidad por línea de producto
-- ============================================================
SELECT
    cc.nombre                       AS Categoria,
    COUNT(DISTINCT p.producto_id)   AS NumeroProductos,
    ROUND(AVG(p.precio), 2)         AS PrecioPromedioIGVInc,
    ROUND(AVG(p.precio / 1.18), 2)  AS PrecioPromedioSinIGV,
    ROUND(AVG(p.costo), 2)          AS CostoPromedio,
    ROUND(AVG(p.precio / 1.18 - p.costo), 2)          AS MargenUnitarioPromedio,
    ROUND(
        100.0 * AVG(p.precio / 1.18 - p.costo)
              / NULLIF(AVG(p.precio / 1.18), 0)
    , 2)                            AS PorcentajeMargen
FROM Producto p
INNER JOIN CategoriaCafe cc ON p.categoria_id = cc.categoria_id
WHERE p.activo = 1
GROUP BY cc.categoria_id, cc.nombre
ORDER BY PorcentajeMargen DESC;
GO

-- ============================================================
-- CONSULTA 15 — Horas pico de venta por sede
-- Tipo: JOIN + COUNT + GROUP BY + ORDER BY
-- Uso gerencial: optimizar horarios de apertura y refuerzos de personal
-- ============================================================
SELECT
    s.nombre                        AS Sede,
    DATEPART(HOUR, t.fecha)         AS Hora,
    COUNT(t.transaccion_id)         AS NumeroVentas,
    SUM(t.total)                    AS TotalRecaudado,
    ROUND(AVG(t.total), 2)          AS TicketPromedio
FROM Transaccion t
INNER JOIN Sede s ON t.sede_id = s.sede_id
GROUP BY s.sede_id, s.nombre, DATEPART(HOUR, t.fecha)
ORDER BY s.nombre, NumeroVentas DESC;
GO

-- ============================================================
-- CONSULTA BONUS — Composición IGV de las ventas del período
-- Tipo: subconsulta + expresión aritmética
-- Uso contable: verificación del monto a declarar ante SUNAT
-- ============================================================
SELECT
    CAST(t.fecha AS DATE)           AS Dia,
    SUM(t.subtotal)                 AS BaseImponible,
    SUM(t.impuesto)                 AS IGV_Declarar,
    SUM(t.total)                    AS TotalFacturado,
    -- Verificación: base * 1.18 debe ≈ total (diferencia < 0.02 por redondeo)
    ROUND(SUM(t.subtotal) * 1.18, 2) AS BaseX118_Verificacion
FROM Transaccion t
GROUP BY CAST(t.fecha AS DATE)
ORDER BY Dia;
GO

-- ============================================================
-- FIN DEL SCRIPT — 15 consultas de gestión + 1 bonus
-- ============================================================
