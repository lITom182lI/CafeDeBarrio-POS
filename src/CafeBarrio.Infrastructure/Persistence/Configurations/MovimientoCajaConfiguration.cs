using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("MovimientoCaja");
        builder.HasKey(m => m.MovimientoCajaId);
        builder.Property(m => m.TipoMovimiento).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Monto).HasPrecision(10, 2).IsRequired();
        builder.Property(m => m.FechaHora).IsRequired();

        builder.HasOne(m => m.Turno)
               .WithMany(t => t.Movimientos)
               .HasForeignKey(m => m.TurnoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.TurnoId);
    }
}
