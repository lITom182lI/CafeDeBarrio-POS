using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class AnulacionConfiguration : IEntityTypeConfiguration<Anulacion>
{
    public void Configure(EntityTypeBuilder<Anulacion> builder)
    {
        builder.ToTable("Anulacion");
        builder.HasKey(a => a.AnulacionId);
        builder.Property(a => a.TipoAnulacion).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Motivo).IsRequired().HasMaxLength(200);
        builder.Property(a => a.MontoOriginal).HasPrecision(10, 2).IsRequired();
        builder.Property(a => a.MontoDevuelto).HasPrecision(10, 2).IsRequired();
        builder.Property(a => a.MetodoDevolucion).HasMaxLength(50);
        builder.Property(a => a.FechaHora).IsRequired();
        builder.Property(a => a.ImpactoInventario).HasDefaultValue(true);

        builder.HasOne(a => a.Transaccion)
               .WithOne(t => t.Anulacion)
               .HasForeignKey<Anulacion>(a => a.TransaccionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.OperadorSolicitante)
               .WithMany(o => o.AnulacionesSolicitadas)
               .HasForeignKey(a => a.OperadorSolicitanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Autorizador)
               .WithMany(o => o.AnulacionesAutorizadas)
               .HasForeignKey(a => a.AutorizadorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
