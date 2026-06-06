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
    public DbSet<Operador> Operadores => Set<Operador>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<Anulacion> Anulaciones => Set<Anulacion>();
    public DbSet<ConfiguracionNegocio> ConfiguracionesNegocio => Set<ConfiguracionNegocio>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CafeBarrioDbContext).Assembly);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Rol).HasMaxLength(20);
            e.Property(u => u.Email).HasMaxLength(100);
            e.Property(u => u.PasswordHash).HasMaxLength(256);
        });

        modelBuilder.Entity<Transaccion>(e =>
        {
            e.Property(t => t.CreatedAt)
             .HasColumnName("created_at")
             .HasDefaultValueSql("GETUTCDATE()");
            e.Property(t => t.UpdatedAt)
             .HasColumnName("updated_at")
             .IsRequired(false);
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.Property(p => p.CreatedAt)
             .HasColumnName("created_at")
             .HasDefaultValueSql("GETUTCDATE()");
            e.Property(p => p.UpdatedAt)
             .HasColumnName("updated_at")
             .IsRequired(false);
        });
    }
}
