# Smoke Test — Instalador Cafe de Barrio en PC Limpia
Fecha de referencia: 2026-06-06
Version: v2.0
Ejecutar ante: nueva instalacion, actualizacion de version, nueva maquina.

---

## Requisitos de la maquina de prueba
- Windows 10 Pro / Windows 11 (64-bit)
- 8 GB RAM minimo (16 GB recomendado para Docker)
- 20 GB espacio libre en disco
- Conexion a red local (misma red que el servidor si se prueba modo LAN)
- Sin instalacion previa de Cafe de Barrio

---

## Paso 1 — Infraestructura base

| # | Accion | Resultado esperado | OK |
|---|--------|-------------------|----|
| 1.1 | Instalar Docker Desktop (descarga oficial) | Docker inicia sin errores | [ ] |
| 1.2 | Ejecutar: docker pull mcr.microsoft.com/mssql/server:2022-latest | Imagen descargada | [ ] |
| 1.3 | Ejecutar: docker run -e ACCEPT_EULA=Y -e SA_PASSWORD="Muis_CafeBarrio_2026!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest | Contenedor activo (docker ps) | [ ] |

## Paso 2 — Base de datos y API

| # | Accion | Resultado esperado | OK |
|---|--------|-------------------|----|
| 2.1 | Publicar API con build-release.ps1 o abrir en Visual Studio | Carpeta publish generada | [ ] |
| 2.2 | Configurar ConnectionStrings en appsettings.json (apuntar al sqlserver local) | Archivo editado | [ ] |
| 2.3 | Ejecutar la API | Consola muestra "Now listening on https://..." | [ ] |
| 2.4 | GET https://localhost:7240/api/categorias | Responde 200 + array de categorias | [ ] |
| 2.5 | Verificar que el seeder corrió | BD CafeDeBarrio tiene: Sede, TipoCliente, MetodoPago, CategoriaCafe, Cliente "Mostrador", ConfiguracionNegocio, Usuario admin | [ ] |

## Paso 3 — Dashboard

| # | Accion | Resultado esperado | OK |
|---|--------|-------------------|----|
| 3.1 | Abrir http://localhost:5173 (o build de produccion) | Redirige a /login | [ ] |
| 3.2 | Login con admin@cafedebarrio.com / Admin2026! | Ingresa al Dashboard | [ ] |
| 3.3 | Navegar a Productos | Tabla carga (puede estar vacia) | [ ] |
| 3.4 | Crear un producto: "Cafe Americano", precio S/ 5.50, categoria Cafes | Producto aparece en tabla con badge Activo | [ ] |
| 3.5 | Editar el producto: cambiar precio a S/ 6.00 | Precio actualizado en tabla | [ ] |
| 3.6 | Navegar a Operadores → crear operador "Maria" con PIN 1234 | Operador aparece en lista | [ ] |
| 3.7 | Navegar a Configuracion → cambiar password a "Admin2026!!" | Redirige a /login | [ ] |
| 3.8 | Login con nueva password "Admin2026!!" | Ingresa correctamente | [ ] |

## Paso 4 — POS WinForms

| # | Accion | Resultado esperado | OK |
|---|--------|-------------------|----|
| 4.1 | Ejecutar el instalador CafeDeBarrio_Setup.exe | Se instala en C:\Program Files\CafeDeBarrio\ | [ ] |
| 4.2 | Abrir App.config con ApiBaseUrl vacio o "AQUI" | MessageBox "Configuracion inicial requerida" → app cierra | [ ] |
| 4.3 | Editar App.config: ApiBaseUrl=https://localhost:7240, AcceptSelfSigned=true, PrintTicket=true | Guardado | [ ] |
| 4.4 | Abrir POS | Carga catalogo, muestra boton "Cafe Americano" | [ ] |
| 4.5 | Agregar Cafe Americano al carrito, seleccionar Efectivo, clic COBRAR | MessageBox "Venta completada" + diálogo de impresion | [ ] |
| 4.6 | Verificar en Dashboard > Transacciones | La venta aparece en la lista | [ ] |
| 4.7 | Desconectar API (detener el proceso) → realizar otra venta en POS | MessageBox "guardada localmente" + status "Sin conexion" | [ ] |
| 4.8 | Reconectar API → esperar 30s | Status POS vuelve a "API conectada", transaccion sincronizada | [ ] |

## Paso 5 — Seguridad

| # | Accion | Resultado esperado | OK |
|---|--------|-------------------|----|
| 5.1 | POST /api/auth/login 6 veces con password erronea | 6ta respuesta: 429 Too Many Requests con Retry-After: 300 | [ ] |
| 5.2 | POST /api/productos sin Authorization header | 401 Unauthorized | [ ] |
| 5.3 | GET /api/productos sin Authorization header | 200 OK (endpoint publico para POS) | [ ] |

---

## Criterio de aprobacion
Todas las casillas marcadas [X].
Documentar cualquier desviacion en la columna "Resultado esperado".
Firma del evaluador: _______________  Fecha: _______________
