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
    }
}
