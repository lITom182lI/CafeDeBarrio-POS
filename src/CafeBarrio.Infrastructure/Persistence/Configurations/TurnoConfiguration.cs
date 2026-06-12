using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("Turno");
        builder.HasKey(t => t.TurnoId);
        builder.Property(t => t.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Abierto");
        builder.Property(t => t.MontoApertura).HasPrecision(10, 2).IsRequired();
        builder.Property(t => t.MontoEfectivoCierto).HasPrecision(10, 2);
        builder.Property(t => t.TotalEfectivoSistema).HasPrecision(10, 2);
        builder.Property(t => t.FechaApertura).IsRequired();
        builder.Property(t => t.Observaciones).HasMaxLength(500);
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasOne(t => t.Sede)
               .WithMany()
               .HasForeignKey(t => t.SedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Operador)
               .WithMany(o => o.Turnos)
               .HasForeignKey(t => t.OperadorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.SedeId);
        builder.HasIndex(t => t.FechaApertura);
    }
}
