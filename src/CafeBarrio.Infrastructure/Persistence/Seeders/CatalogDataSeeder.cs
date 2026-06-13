using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CafeBarrio.Infrastructure.Persistence.Seeders;

public interface ICatalogDataSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}

public class CatalogDataSeeder : ICatalogDataSeeder
{
    private readonly CafeBarrioDbContext _context;
    private readonly CafeBarrio.Application.Common.Interfaces.IPasswordHasher _hasher;
    private readonly Microsoft.Extensions.Hosting.IHostEnvironment _env;
    private readonly IConfiguration _config;

    public CatalogDataSeeder(
        CafeBarrioDbContext context,
        CafeBarrio.Application.Common.Interfaces.IPasswordHasher hasher,
        Microsoft.Extensions.Hosting.IHostEnvironment env,
        IConfiguration config)
    {
        _context = context;
        _hasher  = hasher;
        _env     = env;
        _config  = config;
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
            _context.Sedes.Add(sede);
            await _context.SaveChangesAsync(ct);
        }

        // 2. TiposCliente
        bool tiposExisten = await _context.TiposCliente.AnyAsync(ct);
        if (!tiposExisten)
        {
            _context.TiposCliente.Add(new TipoCliente { Nombre = "Regular" });
            await _context.SaveChangesAsync(ct);
        }

        // 3. MetodosPago
        bool metodosExisten = await _context.MetodosPago.AnyAsync(ct);
        if (!metodosExisten)
        {
            _context.MetodosPago.AddRange(
                new MetodoPago { Nombre = "Efectivo", Activo = true, EsEfectivo = true  },
                new MetodoPago { Nombre = "Tarjeta",  Activo = true, EsEfectivo = false },
                new MetodoPago { Nombre = "Yape",     Activo = true, EsEfectivo = false },
                new MetodoPago { Nombre = "Plin",     Activo = true, EsEfectivo = false }
            );
            await _context.SaveChangesAsync(ct);
        }

        // 4. CategoriasCafe
        bool categoriasExisten = await _context.CategoriasCafe.AnyAsync(ct);
        if (!categoriasExisten)
        {
            var categorias = new List<CategoriaCafe>
            {
                new CategoriaCafe { Codigo = "CAF", Nombre = "Cafes",   Descripcion = "Cafés especiales y de origen", Activa = true },
                new CategoriaCafe { Codigo = "BEB", Nombre = "Bebidas", Descripcion = "Cafés calientes y bebidas frías", Activa = true },
                new CategoriaCafe { Codigo = "COM", Nombre = "Comida",  Descripcion = "Snacks, postres y sandwiches", Activa = true }
            };
            _context.CategoriasCafe.AddRange(categorias);
            await _context.SaveChangesAsync(ct);
        }

        // 5. Cliente Mostrador
        bool mostradorExiste = await _context.Clientes.AnyAsync(c => c.Email == "mostrador@cafedebarrio.local", ct);
        if (!mostradorExiste)
        {
            var tipoId = await _context.TiposCliente.Select(t => t.TipoClienteId).FirstOrDefaultAsync(ct);
            _context.Clientes.Add(new Cliente
            {
                TipoClienteId = tipoId > 0 ? tipoId : 1,
                Nombre        = "Mostrador",
                Apellido      = string.Empty,
                Email         = "mostrador@cafedebarrio.local",
                FechaRegistro = DateOnly.FromDateTime(DateTime.UtcNow),
                Activo        = true
            });
            await _context.SaveChangesAsync(ct);
        }

        // 6. ConfiguracionNegocio
        var configExiste = await _context.ConfiguracionesNegocio.AnyAsync(c => c.SedeId == sede.SedeId && c.Activo, ct);
        if (!configExiste)
        {
            _context.ConfiguracionesNegocio.Add(new ConfiguracionNegocio
            {
                SedeId = sede.SedeId,
                TasaIGV = 0.16m,
                TasaIPM = 0.02m,
                FechaVigencia = new DateTime(2026, 1, 1),
                Activo = true
            });
            await _context.SaveChangesAsync(ct);
        }

        // 7. Usuario Admin
        bool adminExiste = await _context.Usuarios.AnyAsync(ct);
        if (!adminExiste)
        {
            if (!_env.IsDevelopment())
            {
                var pwd = _config["Seed:AdminPassword"];
                if (string.IsNullOrWhiteSpace(pwd) || pwd.StartsWith("OVERRIDE_VIA_ENV_VAR") || pwd.StartsWith("REEMPLAZAR_"))
                {
                    throw new InvalidOperationException("Seed:AdminPassword no configurado.");
                }
            }

            _context.Usuarios.Add(new Usuario
            {
                Email        = "admin@cafedebarrio.com",
                PasswordHash = _hasher.Hash(
                    _config["Seed:AdminPassword"]
                    ?? throw new InvalidOperationException("Seed:AdminPassword no configurado.")),
                Rol          = "Admin",
                Activo       = true
            });
            await _context.SaveChangesAsync(ct);
        }

        // 8. Productos
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
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Espresso Doble Clásico", Descripcion = "Espresso intenso de doble carga.", Costo = 3.50m, Precio = 7.00m, CantidadDisponible = 80m, StockMinimo = 20m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Latte Vainilla de Barrio", Descripcion = "Café latte con jarabe de vainilla.", Costo = 4.00m, Precio = 9.50m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Capuccino Canela", Descripcion = "Espresso con leche espumosa.", Costo = 3.80m, Precio = 8.50m, CantidadDisponible = 70m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Mocca Chocolate Amargo", Descripcion = "Espresso, leche y salsa de chocolate.", Costo = 4.20m, Precio = 10.00m, CantidadDisponible = 50m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Americano de la Casa", Descripcion = "Café americano suave.", Costo = 2.80m, Precio = 6.50m, CantidadDisponible = 90m, StockMinimo = 25m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Cold Brew Cítrico", Descripcion = "Cold brew de 12 horas.", Costo = 4.50m, Precio = 11.00m, CantidadDisponible = 40m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Frappe Caramelo Salado", Descripcion = "Café frappé con caramelo salado.", Costo = 4.70m, Precio = 11.50m, CantidadDisponible = 35m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Limonada con Hierbabuena", Descripcion = "Limonada frutada.", Costo = 2.50m, Precio = 6.00m, CantidadDisponible = 80m, StockMinimo = 20m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catBebidas.CategoriaId, Nombre = "Té Helado Durazno", Descripcion = "Té negro frío sabor durazno.", Costo = 2.40m, Precio = 6.00m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    // Cafes
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Latte Avellana Rock", Descripcion = "Latte con jarabe de avellana.", Costo = 4.30m, Precio = 10.50m, CantidadDisponible = 45m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Café Nitro Barril", Descripcion = "Café frío infusionado con nitrógeno.", Costo = 5.20m, Precio = 12.50m, CantidadDisponible = 30m, StockMinimo = 8m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catCafes.CategoriaId, Nombre = "Affogato de Vainilla", Descripcion = "Bola de helado de vainilla.", Costo = 4.80m, Precio = 11.00m, CantidadDisponible = 35m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    // Comida
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Brownie de Chocolate Intenso", Descripcion = "Brownie casero de chocolate.", Costo = 2.80m, Precio = 7.00m, CantidadDisponible = 50m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Cheesecake de Maracuyá", Descripcion = "Porción de cheesecake.", Costo = 3.80m, Precio = 9.50m, CantidadDisponible = 30m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Sándwich de Pollo BBQ", Descripcion = "Pan artesanal con pollo.", Costo = 4.00m, Precio = 10.00m, CantidadDisponible = 40m, StockMinimo = 10m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() },
                    new Producto { CategoriaId = catComida.CategoriaId, Nombre = "Croissant de Mantequilla", Descripcion = "Croissant horneado del día.", Costo = 2.20m, Precio = 5.50m, CantidadDisponible = 60m, StockMinimo = 15m, UnidadMedida = "Unidades", SeguimientoInventario = true, EsMayorista = false, Activo = true, CreatedAt = ahora, RowVersion = Array.Empty<byte>() }
                };

                _context.Productos.AddRange(productos);
                await _context.SaveChangesAsync(ct);
            }
        }

        var metodoEfec = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == "Efectivo", ct);
        var metodoYape = await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == "Yape", ct);

        // 9. Test Data Simulation — Protegido por entorno (MUIS Best Practice)
        bool isDemo = _env.IsDevelopment() || Environment.GetEnvironmentVariable("SEED_DEMO_DATA") == "true";
        if (!isDemo) return;

        bool hasOps = await _context.Operadores.CountAsync(ct) >= 10;
        List<Operador> ops = hasOps ? await _context.Operadores.ToListAsync(ct) : new List<Operador>();
        
        if (!hasOps)
        {
            // Generar 10 operadores
            for(int i = 1; i <= 10; i++)
            {
                ops.Add(new Operador { 
                    SedeId = sede.SedeId, 
                    Nombre = $"Test Operador {i}", 
                    PinHash = _hasher.Hash("123456"), 
                    Activo = true, 
                    CreatedAt = DateTime.UtcNow 
                });
            }
            _context.Operadores.AddRange(ops);
            await _context.SaveChangesAsync(ct);
        }

        bool tieneAbierto = await _context.Turnos.AnyAsync(t => t.SedeId == sede.SedeId && t.Estado == "Abierto", ct);
        bool hasTurnos = tieneAbierto || await _context.Turnos.CountAsync(ct) >= 10;
        List<Turno> turnos = hasTurnos ? await _context.Turnos.ToListAsync(ct) : new List<Turno>();
        if (!hasTurnos)
        {
            // Generar 10 turnos
            for(int i = 0; i < 10; i++)
            {
                var t = new Turno {
                    OperadorId = ops[i].OperadorId,
                    SedeId = sede.SedeId,
                    FechaApertura = DateTime.UtcNow.AddDays(-30 + i * 3),
                    MontoApertura = 100m,
                    Estado = i == 9 ? "Abierto" : "Cerrado",
                    FechaCierre = i == 9 ? null : DateTime.UtcNow.AddDays(-30 + i * 3).AddHours(8),
                    MontoEfectivoCierto = i == 9 ? null : 500m,
                    TotalEfectivoSistema = i == 9 ? null : 500m
                };
                turnos.Add(t);
            }
            _context.Turnos.AddRange(turnos);
            await _context.SaveChangesAsync(ct);
        }

        var allProds = await _context.Productos.ToListAsync(ct);
        if (allProds.Any() && metodoEfec != null && metodoYape != null && turnos.Any())
        {
            bool hasVentas = await _context.Transacciones.CountAsync(ct) >= 100;
            if (!hasVentas)
            {
                // Generar 100 Ventas Ficticias distribuidas en los últimos 30 días
                var random = new Random(42);
                var ventas = new List<Transaccion>();
                for(int i = 0; i < 100; i++)
                {
                    var prodIndex = random.Next(allProds.Count);
                    var prod = allProds[prodIndex];
                    var qty = random.Next(1, 5); // 1 a 4 productos
                    var method = random.Next(2) == 0 ? metodoEfec.MetodoPagoId : metodoYape.MetodoPagoId;
                    
                    var diasAtras = random.Next(0, 30);
                    var horasAtras = random.Next(8, 20); // Entre 8am y 8pm
                    var fechaVenta = DateTime.UtcNow.AddDays(-diasAtras).Date.AddHours(horasAtras).AddMinutes(random.Next(0, 60));

                    var v = new Transaccion {
                        SedeId = sede.SedeId,
                        TurnoId = turnos[random.Next(turnos.Count)].TurnoId,
                        OperadorId = ops[random.Next(ops.Count)].OperadorId,
                        MetodoPagoId = method,
                        Fecha = fechaVenta,
                        Subtotal = prod.Precio * qty * 0.82m,
                        Impuesto = prod.Precio * qty * 0.18m,
                        Total = prod.Precio * qty,
                        Canal = "Local",
                        CreatedAt = fechaVenta,
                        SunatEstado = "Aceptado",
                        Detalles = new List<DetalleTransaccion> {
                            new DetalleTransaccion {
                                ProductoId = prod.ProductoId,
                                Cantidad = qty,
                                PrecioUnitario = prod.Precio,
                                SubtotalLinea = prod.Precio * qty
                            }
                        }
                    };
                    ventas.Add(v);
                }
                _context.Transacciones.AddRange(ventas);
                await _context.SaveChangesAsync(ct);
                
                // Generar 10 Anulaciones
                var anulaciones = new List<Anulacion>();
                for(int i = 0; i < 10; i++)
                {
                    var v = ventas[i];
                    v.SunatEstado = "Anulado"; // Actualizamos estado de la transacción
                    
                    anulaciones.Add(new Anulacion {
                        TransaccionId = v.TransaccionId,
                        OperadorSolicitanteId = ops[2].OperadorId,
                        AutorizadorId = ops[0].OperadorId,
                        FechaHora = v.Fecha.AddMinutes(30),
                        TipoAnulacion = "DevolucionTotal",
                        Motivo = "Error en el pedido de prueba",
                        MontoOriginal = v.Total,
                        MontoDevuelto = v.Total,
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
