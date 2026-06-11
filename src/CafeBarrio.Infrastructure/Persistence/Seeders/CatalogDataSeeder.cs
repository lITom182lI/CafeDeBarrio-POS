using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence.Seeders;

public interface ICatalogDataSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}

public class CatalogDataSeeder : ICatalogDataSeeder
{
    private readonly CafeBarrioDbContext _context;

    public CatalogDataSeeder(CafeBarrioDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // 1. Sembrar Sede 1 si no existe
        var sede = await _context.Sedes.FirstOrDefaultAsync(s => s.SedeId == 1, ct);
        if (sede == null)
        {
            sede = new Sede
            {
                Nombre = "Café de Barrio",
                Direccion = "Av. Principal 123",
                Distrito = "Miraflores",
                Ciudad = "Lima",
                EsPrincipal = true,
                Activa = true,
                FechaApertura = new DateOnly(2026, 1, 1)
            };
            // Forzamos el ID si es necesario o dejamos que IDENTITY asigne 1
            // Si la tabla está vacía IDENTITY(1,1) le dará 1.
            _context.Sedes.Add(sede);
            await _context.SaveChangesAsync(ct);
        }

        // 2. Sembrar ConfiguraciónNegocio para Sede 1 si no existe
        var configExiste = await _context.ConfiguracionesNegocio.AnyAsync(c => c.SedeId == sede.SedeId && c.Activo, ct);
        if (!configExiste)
        {
            _context.ConfiguracionesNegocio.Add(new ConfiguracionNegocio
            {
                SedeId = sede.SedeId,
                TasaIGV = 0.18m,
                TasaIPM = 0.02m,
                FechaVigencia = new DateTime(2026, 1, 1),
                Activo = true
            });
            await _context.SaveChangesAsync(ct);
        }

        // 3. Sembrar Categorías si la tabla está vacía
        bool categoriasExisten = await _context.CategoriasCafe.AnyAsync(ct);
        if (!categoriasExisten)
        {
            var categorias = new List<CategoriaCafe>
            {
                new CategoriaCafe { Codigo = "BEB", Nombre = "Bebidas", Descripcion = "Cafés calientes y bebidas frías", Activa = true },
                new CategoriaCafe { Codigo = "CAF", Nombre = "Cafes", Descripcion = "Cafés especiales y de origen", Activa = true },
                new CategoriaCafe { Codigo = "COM", Nombre = "Comida", Descripcion = "Snacks, postres y sandwiches", Activa = true }
            };
            _context.CategoriasCafe.AddRange(categorias);
            await _context.SaveChangesAsync(ct);
        }

        // 4. Sembrar Productos si la tabla está vacía
        bool productosExisten = await _context.Productos.AnyAsync(ct);
        if (!productosExisten)
        {
            var catBebidas = await _context.CategoriasCafe.FirstOrDefaultAsync(c => c.Codigo == "BEB", ct);
            var catCafes = await _context.CategoriasCafe.FirstOrDefaultAsync(c => c.Codigo == "CAF", ct);
            var catComida = await _context.CategoriasCafe.FirstOrDefaultAsync(c => c.Codigo == "COM", ct);

            if (catBebidas != null && catCafes != null && catComida != null)
            {
                var ahora = DateTime.UtcNow;
                var productos = new List<Producto>
                {
                    // Bebidas
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Espresso Doble Clásico", Descripcion = "Espresso intenso de doble carga, ideal para los que necesitan energía extra.", Costo = 3.50m, Precio = 7.00m, CantidadDisponible = 80m, StockMinimo = 20m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Latte Vainilla de Barrio", Descripcion = "Café latte con jarabe de vainilla y leche vaporizada cremosa.", Costo = 4.00m, Precio = 9.50m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Capuccino Canela", Descripcion = "Espresso con leche espumosa, toque de canela y cacao espolvoreado.", Costo = 3.80m, Precio = 8.50m, CantidadDisponible = 70m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Mocca Chocolate Amargo", Descripcion = "Espresso, leche y salsa de chocolate amargo, coronado con crema batida.", Costo = 4.20m, Precio = 10.00m, CantidadDisponible = 50m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Americano de la Casa", Descripcion = "Café americano suave, preparado con mezcla de la casa.", Costo = 2.80m, Precio = 6.50m, CantidadDisponible = 90m, StockMinimo = 25m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Cold Brew Cítrico", Descripcion = "Cold brew de 12 horas con rodajas de naranja y toque de tónica.", Costo = 4.50m, Precio = 11.00m, CantidadDisponible = 40m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Frappe Caramelo Salado", Descripcion = "Café frappé con caramelo salado, hielo licuado y crema batida.", Costo = 4.70m, Precio = 11.50m, CantidadDisponible = 35m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Limonada con Hierbabuena", Descripcion = "Limonada frutada con hojas de hierbabuena fresca.", Costo = 2.50m, Precio = 6.00m, CantidadDisponible = 80m, StockMinimo = 20m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Té Helado Durazno", Descripcion = "Té negro frío sabor durazno, ligeramente endulzado.", Costo = 2.40m, Precio = 6.00m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    // Cafes
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Latte Avellana Rock", Descripcion = "Latte con jarabe de avellana, inspirado en el estilo rock del café de barrio.", Costo = 4.30m, Precio = 10.50m, CantidadDisponible = 45m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Café Nitro Barril", Descripcion = "Café frío infusionado con nitrógeno, servido directo del barril.", Costo = 5.20m, Precio = 12.50m, CantidadDisponible = 30m, StockMinimo = 8m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Affogato de Vainilla", Descripcion = "Bola de helado de vainilla bañada con espresso caliente.", Costo = 4.80m, Precio = 11.00m, CantidadDisponible = 35m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    // Comida
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Brownie de Chocolate Intenso", Descripcion = "Brownie casero de chocolate 70% cacao.", Costo = 2.80m, Precio = 7.00m, CantidadDisponible = 50m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Cheesecake de Maracuyá", Descripcion = "Porción de cheesecake con coulis de maracuyá.", Costo = 3.80m, Precio = 9.50m, CantidadDisponible = 30m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Sándwich de Pollo BBQ", Descripcion = "Pan artesanal con pollo deshilachado en salsa BBQ y queso.", Costo = 4.00m, Precio = 10.00m, CantidadDisponible = 40m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Croissant de Mantequilla", Descripcion = "Croissant horneado del día, masa hojaldrada y ligera.", Costo = 2.20m, Precio = 5.50m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() }
                };

                _context.Productos.AddRange(productos);
                await _context.SaveChangesAsync(ct);
            }
        }

        // 5. Metodos de pago (si no existen)
        bool metodosExisten = await _context.MetodosPago.AnyAsync(ct);
        if (!metodosExisten)
        {
            _context.MetodosPago.AddRange(
                new MetodoPago { Nombre = "Efectivo", Activo = true },
                new MetodoPago { Nombre = "Yape", Activo = true },
                new MetodoPago { Nombre = "Plin", Activo = true },
                new MetodoPago { Nombre = "Tarjeta", Activo = true }
            );
            await _context.SaveChangesAsync(ct);
        }

        var metodoEfec = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == "Efectivo", ct);
        var metodoYape = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == "Yape", ct);

        // 6. Test Data Simulation (Operadores, Turnos, Transacciones, Anulaciones)
        bool hasTestData = await _context.Operadores.CountAsync(ct) >= 10;
        if (!hasTestData)
        {
            // Generar 10 operadores
            var ops = new List<Operador>();
            for(int i = 1; i <= 10; i++)
            {
                ops.Add(new Operador { 
                    SedeId = sede.SedeId, 
                    Nombre = $"Test Operador {i}", 
                    PinHash = "123456", 
                    Activo = true, 
                    CreatedAt = DateTime.UtcNow 
                });
            }
            _context.Operadores.AddRange(ops);
            await _context.SaveChangesAsync(ct);

            // Generar 10 turnos (9 cerrados, 1 abierto)
            var turnos = new List<Turno>();
            for(int i = 0; i < 10; i++)
            {
                var t = new Turno {
                    OperadorId = ops[i].OperadorId,
                    SedeId = sede.SedeId,
                    FechaApertura = DateTime.UtcNow.AddDays(-10 + i),
                    MontoApertura = 100m,
                    Estado = i == 9 ? "Abierto" : "Cerrado",
                    FechaCierre = i == 9 ? null : DateTime.UtcNow.AddDays(-10 + i).AddHours(8),
                    MontoEfectivoCierto = i == 9 ? null : 500m,
                    TotalEfectivoSistema = i == 9 ? null : 500m
                };
                turnos.Add(t);
            }
            _context.Turnos.AddRange(turnos);
            await _context.SaveChangesAsync(ct);

            var allProds = await _context.Productos.ToListAsync(ct);
            if (allProds.Any() && metodoEfec != null && metodoYape != null)
            {
                // Generar 10 Ventas Completadas
                var ventas = new List<Transaccion>();
                for(int i = 0; i < 10; i++)
                {
                    var v = new Transaccion {
                        SedeId = sede.SedeId,
                        TurnoId = turnos[0].TurnoId,
                        OperadorId = ops[0].OperadorId,
                        MetodoPagoId = i % 2 == 0 ? metodoEfec.MetodoPagoId : metodoYape.MetodoPagoId,
                        Fecha = DateTime.UtcNow.AddDays(-5).AddHours(i),
                        Subtotal = 10m,
                        Impuesto = 1.8m,
                        Total = 11.8m,
                        Canal = "Local",
                        CreatedAt = DateTime.UtcNow,
                        SunatEstado = "Aceptado",
                        Detalles = new List<DetalleTransaccion> {
                            new DetalleTransaccion {
                                ProductoId = allProds[0].ProductoId,
                                Cantidad = 1,
                                PrecioUnitario = 10m,
                                SubtotalLinea = 10m
                            }
                        }
                    };
                    ventas.Add(v);
                }
                _context.Transacciones.AddRange(ventas);
                await _context.SaveChangesAsync(ct);

                // Generar 10 Ventas para luego ser Anuladas
                var ventasParaAnular = new List<Transaccion>();
                for(int i = 0; i < 10; i++)
                {
                    var v = new Transaccion {
                        SedeId = sede.SedeId,
                        TurnoId = turnos[1].TurnoId,
                        OperadorId = ops[1].OperadorId,
                        MetodoPagoId = metodoEfec.MetodoPagoId,
                        Fecha = DateTime.UtcNow.AddDays(-2).AddHours(i),
                        Subtotal = 20m,
                        Impuesto = 3.6m,
                        Total = 23.6m,
                        Canal = "Local",
                        CreatedAt = DateTime.UtcNow,
                        SunatEstado = "Anulado",
                        Detalles = new List<DetalleTransaccion> {
                            new DetalleTransaccion {
                                ProductoId = allProds[1].ProductoId,
                                Cantidad = 2,
                                PrecioUnitario = 10m,
                                SubtotalLinea = 20m
                            }
                        }
                    };
                    ventasParaAnular.Add(v);
                }
                _context.Transacciones.AddRange(ventasParaAnular);
                await _context.SaveChangesAsync(ct);

                // Crear las 10 Anulaciones
                var anulaciones = new List<Anulacion>();
                for(int i = 0; i < 10; i++)
                {
                    anulaciones.Add(new Anulacion {
                        TransaccionId = ventasParaAnular[i].TransaccionId,
                        OperadorSolicitanteId = ops[2].OperadorId,
                        AutorizadorId = ops[0].OperadorId,
                        FechaHora = ventasParaAnular[i].Fecha.AddMinutes(30),
                        TipoAnulacion = "DevolucionTotal",
                        Motivo = "Error en el pedido de prueba",
                        MontoOriginal = ventasParaAnular[i].Total,
                        MontoDevuelto = ventasParaAnular[i].Total,
                        MetodoDevolucion = "Efectivo",
                        ImpactoInventario = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                _context.Anulaciones.AddRange(anulaciones);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
