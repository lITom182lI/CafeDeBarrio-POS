using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CafeBarrio.Infrastructure.Persistence;

public class CafeBarrioDbContext : DbContext
{
    public CafeBarrioDbContext(DbContextOptions<CafeBarrioDbContext> options) : base(options) { }

    public DbSet<CategoriaCafe> CategoriasCafe => Set<CategoriaCafe>();
    public DbSet<TipoCliente> TiposCliente => Set<TipoCliente>();
    public DbSet<UbicacionPreferencia> UbicacionesPreferencia => Set<UbicacionPreferencia>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<OpcionEnvio> OpcionesEnvio => Set<OpcionEnvio>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Sede> Sedes => Set<Sede>();
    public DbSet<Transporte> Transportes => Set<Transporte>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<DetalleTransaccion> DetallesTransaccion => Set<DetalleTransaccion>();
    public DbSet<TransaccionMayorista> TransaccionesMayoristas => Set<TransaccionMayorista>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CafeBarrioDbContext).Assembly);
}
