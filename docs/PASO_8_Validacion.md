# Paso 8 — Validación y Documentación
## Sistema: Café de Barrio POS
**Alumnos:** Pablo Joel Castillo Flores, Justhin Christofher Huisa Valle, Jeremy Geraldo Armas Camones, Geradth Humberto Gaitan Gonzales, Allison Isabel Cordova Diaz
**Fecha:** 2026-07-09 · **Motor:** SQL Server 2022

---

## 1. Metodología de Validación

Cada consulta del **Paso 7** fue ejecutada contra la base de datos `CafeDeBarrio` cargada con los datos de prueba del **Paso 6 (DML)**. El resultado obtenido se validó para comprobar la coherencia del modelo relacional final, la consistencia de los datos y el cumplimiento de las restricciones de integridad.

---

## 2. Tabla Resumen de Validación

| # | Consulta | Objetivo | Resultado Obtenido | ✓ |
|---|---|---|---|:---:|
| 1 | Empleados con su respectiva sede, turno y cargo | Verificar que las llaves foráneas unan correctamente el perfil del trabajador con el catálogo del negocio. | 3 registros devueltos con datos cruzados de Sedes, Turnos y Cargos. | ✓ |
| 2 | Reporte de ventas mostrando el cliente y monto total | Comprobar que la relación Cliente -> Venta -> Detalle_Venta calcule el `MontoTotal` usando multiplicaciones sobre la cantidad y el precio. | Venta 1 (16.00) y Venta 2 (45.00) agrupadas correctamente. | ✓ |
| 3 | Ventas con envío, empresa de transporte y costo | Corroborar el uso correcto de las llaves compuestas en la tabla ENVIA. | 1 registro de envío (Delivery Express) para la Venta 2. | ✓ |
| 4 | Productos suministrados por proveedor y precios | Comprobar la relación muchos a muchos entre Proveedor y Producto mediante la tabla intermedia SUMINISTRA. | 2 registros de suministro activos desde "Finca El Cafetal". | ✓ |
| 5 | Devoluciones con detalle de producto y empleado responsable | Asegurar la trazabilidad de devoluciones y su justificación por empleado. | 1 devolución procesada de un Café Americano por "Bebida fría". | ✓ |
| 6 | Total recaudado agrupado por tipos de pago | Utilizar funciones de agregación (`COUNT`, `SUM`) comprobando la asociación de cobro por venta. | Yape (S/ 16.00) y Tarjeta de Crédito (S/ 45.00). | ✓ |
| 7 | Detalles específicos de Café en Grano con Origen | Verificar herencia de tablas (`CAFE_GRANO`) y asociación a regiones específicas de cultivo. | 1 registro de Café en Grano de Cusco (Perú) con proceso Lavado. | ✓ |

**Resultado: 7/7 consultas correctas (100%)**

---

## 3. Resultados Detallados — Consultas Clave

### Consulta 2 — Reporte de Ventas (Top Ventas)

| NroVenta | Cliente | FechaHora | MontoTotal |
|:---:|---|---|---:|
| 1 | Maria Vargas | 2023-10-01 10:30:00.000 | S/ 16.00 |
| 2 | Juan Salas | 2023-10-02 15:45:00.000 | S/ 45.00 |

> **Análisis:** La suma de los detalles de la venta coinciden perfectamente con los montos registrados en el DML.

### Consulta 6 — Agrupación por Método de Pago

| Tipo_Pago | Cantidad_Transacciones | Total_Cobrado |
|---|:---:|---:|
| Tarjeta de Crédito | 1 | S/ 45.00 |
| Yape | 1 | S/ 16.00 |

> **Análisis:** Permite al negocio llevar un arqueo de caja y conocer rápidamente qué método de pago es más predominante y cuánto recauda.

### Consulta 7 — Origen del Café en Grano

| Nombre_Producto | tipo_grano | proceso | Pais | Region |
|---|---|---|---|---|
| Bolsa Café en Grano | Arábica | Lavado | Perú | Cusco |

> **Análisis:** Refleja la capacidad del modelo de manejar atributos exclusivos para subtipos de productos sin cargar la tabla base `PRODUCTO` con campos que podrían estar vacíos para otras categorías.

---

## 4. Validación de Integridad — Diseño de Base de Datos

El diseño cuenta con un total de **24 tablas** perfectamente normalizadas. El modelo final incorpora mejoras notables de trazabilidad y gestión:

| Componente | Mejora | Comportamiento esperado |
|---|---|---|
| **Herencia de Productos** | Separación entre `CAFE_GRANO` y `CAFE_MOLIDO` vinculados por el `IdProducto` a la tabla base. | Evita valores nulos y asegura la integridad de los metadatos específicos del café. |
| **Normalización (3FN)** | Catálogos extraídos a `TIPO_CLIENTE`, `TIPO_PAGO`, `TIPO_DESCUENTO`, `ORIGEN`. | Reduce redundancia, evita anomalías y unifica valores descriptivos. |
| **Restricción UNIQUE** | DNI / RUC en `CLIENTE` y `PROVEEDOR` (`UNIQUE`). | Bloquea inserciones duplicadas (Ej. Dos proveedores con mismo RUC). |
| **Manejo de Envíos** | Relación `VENTA` a `EMPRESA_TRANSPORTE` usando `ENVIA` como pivote. | Registra costos, guías y estados de envío específicos para pedidos delivery. |

---

## 5. Conclusiones del Sistema

### 5.1 Valor Empresarial Demostrado

1. **Gestión integral del flujo de valor:** Desde que el producto `PROVIENE` de su origen y lo `SUMINISTRA` el proveedor, hasta que el operador `COBRA` y se `ENVIA` al cliente.
2. **Control Gerencial:** Se cuenta con trazabilidad sobre los `Turnos` y los `EMPLEADOS`, sabiendo qué persona aprobó una `DEVOLUCION` de forma explícita.
3. **Escalabilidad:** Al usar tablas de Tipos (clientes, descuentos, pagos), es fácil extender las reglas del negocio sin tocar las tablas transaccionales.

### 5.2 Métricas del sistema implementado

- **24 tablas** normalizadas.
- **Consultas ejecutadas sin errores de JOIN**.
- **0 anomalías** de inserción gracias al respeto estricto del orden del DDL y las claves foráneas.

---

## 6. Capturas de Ejecución

> *(Insertar aquí capturas de pantalla de SQL Server Management Studio (SSMS) ejecutando los scripts del Paso 7)*
>
> **Recomendaciones para las capturas:**
> - **Consulta 2 (Ventas Totales):** Demuestra dominio de uniones (JOINs) y agrupaciones (GROUP BY).
> - **Consulta 4 (Suministros):** Muestra el control de stock y de proveedores.
> - **Consulta 7 (Orígenes):** Certifica la calidad del diseño relacional avanzado que soporta especialización de datos.

---

*Documento actualizado — Asignatura: Base de Datos · Ciclo 3*
