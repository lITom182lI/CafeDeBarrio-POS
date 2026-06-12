using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class OperadorConfiguration : IEntityTypeConfiguration<Operador>
{
    public void Configure(EntityTypeBuilder<Operador> builder)
    {
        builder.ToTable("Operador");
        builder.HasKey(o => o.OperadorId);
        builder.Property(o => o.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(o => o.PinHash).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Activo).HasDefaultValue(true);
        builder.Property(o => o.Eliminado).HasDefaultValue(false);

        builder.HasOne(o => o.Sede)
               .WithMany()
               .HasForeignKey(o => o.SedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(o => !o.Eliminado);
    }
}
