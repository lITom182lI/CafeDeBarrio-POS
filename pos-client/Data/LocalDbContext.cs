using CafeBarrio.POS.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.POS.Data;

public class LocalDbContext : DbContext
{
    public DbSet<PendingTransaccion> PendingTransacciones => Set<PendingTransaccion>();
    public DbSet<CachedProducto> CachedProductos => Set<CachedProducto>();
    public DbSet<CachedMetodoPago> CachedMetodosPago => Set<CachedMetodoPago>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=cafebarrio_local.db");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<CachedProducto>().HasKey(p => p.ProductoId);
        model.Entity<CachedMetodoPago>().HasKey(m => m.MetodoPagoId);
        model.Entity<PendingTransaccion>().HasKey(t => t.Id);
    }
}
