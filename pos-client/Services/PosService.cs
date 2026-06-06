using System.Text.Json;
using CafeBarrio.POS.Data;
using CafeBarrio.POS.Data.Models;
using CafeBarrio.POS.Dtos;

namespace CafeBarrio.POS.Services;

public class PosService
{
    private const decimal TasaIgv = 0.18m;
    private readonly LocalDbContext _db;
    private readonly ApiClient _api;
    private readonly int _sedeId;

    public List<CachedProducto> Productos { get; private set; } = [];
    public List<CachedMetodoPago> MetodosPago { get; private set; } = [];

    public PosService(LocalDbContext db, ApiClient api, int sedeId)
    {
        _db = db;
        _api = api;
        _sedeId = sedeId;
    }

    public async Task CargarCatalogoAsync()
    {
        try
        {
            var prods = await _api.GetProductosAsync();
            var metodos = await _api.GetMetodosPagoAsync();

            // Actualizar caché local
            _db.CachedProductos.RemoveRange(_db.CachedProductos);
            _db.CachedMetodosPago.RemoveRange(_db.CachedMetodosPago);

            _db.CachedProductos.AddRange(prods.Where(p => p.Activo).Select(p => new CachedProducto
            {
                ProductoId = p.ProductoId,
                Nombre = p.Nombre,
                Precio = p.Precio,
                CantidadDisponible = p.CantidadDisponible,
                CategoriaNombre = p.CategoriaNombre ?? "",
                Activo = p.Activo,
                CachedAt = DateTime.UtcNow
            }));

            _db.CachedMetodosPago.AddRange(metodos.Select(m => new CachedMetodoPago
            {
                MetodoPagoId = m.MetodoPagoId,
                Nombre = m.Nombre
            }));

            await _db.SaveChangesAsync();
        }
        catch
        {
            // Sin conexión — usar caché existente
        }

        Productos = [.. _db.CachedProductos.Where(p => p.Activo).OrderBy(p => p.Nombre)];
        MetodosPago = [.. _db.CachedMetodosPago.OrderBy(m => m.MetodoPagoId)];

        // Fallback si nunca se cargaron metodos de pago
        if (MetodosPago.Count == 0)
        {
            MetodosPago =
            [
                new() { MetodoPagoId = 1, Nombre = "Efectivo" },
                new() { MetodoPagoId = 2, Nombre = "Tarjeta" }
            ];
        }
    }

    public (decimal Subtotal, decimal Igv, decimal Total) CalcularTotales(
        List<(int ProductoId, string Nombre, decimal Precio, int Cantidad)> items)
    {
        var subtotal = items.Sum(i => i.Precio * i.Cantidad);
        var igv      = Math.Round(subtotal * TasaIgv, 2);
        var total    = subtotal + igv;
        return (subtotal, igv, total);
    }

    public async Task<bool> RegistrarVentaAsync(
        int metodoPagoId,
        List<(int ProductoId, string Nombre, decimal Precio, int Cantidad)> items)
    {
        var (subtotal, igv, total) = CalcularTotales(items);

        var dtoItems = items.Select(i => new ItemDto(i.ProductoId, i.Cantidad)).ToList();

        var pending = new PendingTransaccion
        {
            SedeId = _sedeId,
            MetodoPagoId = metodoPagoId,
            Subtotal = subtotal,
            Igv = igv,
            Total = total,
            FechaLocal = DateTime.UtcNow,
            ItemsJson = JsonSerializer.Serialize(dtoItems),
            Sincronizada = false
        };

        _db.PendingTransacciones.Add(pending);
        await _db.SaveChangesAsync();

        // Intentar sincronizar inmediatamente
        try
        {
            var request = new CreateTransaccionRequest(_sedeId, null, metodoPagoId, dtoItems);
            var id = await _api.PostTransaccionAsync(request);
            pending.Sincronizada = true;
            pending.FechaSincronizacion = DateTime.UtcNow;
            pending.TransaccionIdServidor = id;
            await _db.SaveChangesAsync();
            return true; // sincronizado al instante
        }
        catch
        {
            return false; // guardado local, se sincronizará después
        }
    }

    public int GetPendienteCount()
        => _db.PendingTransacciones.Count(t => !t.Sincronizada);
}
