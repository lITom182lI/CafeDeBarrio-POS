using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class ConfiguracionNegocioConfiguration : IEntityTypeConfiguration<ConfiguracionNegocio>
{
    public void Configure(EntityTypeBuilder<ConfiguracionNegocio> builder)
    {
        builder.ToTable("ConfiguracionNegocio");
        builder.HasKey(c => c.ConfiguracionNegocioId);
        builder.Property(c => c.TasaIGV).HasPrecision(5, 4).IsRequired();
        builder.Property(c => c.TasaIPM).HasPrecision(5, 4).IsRequired();
        builder.Property(c => c.FechaVigencia).IsRequired();
        builder.Property(c => c.Activo).HasDefaultValue(true);

        builder.HasOne(c => c.Sede)
               .WithMany()
               .HasForeignKey(c => c.SedeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.SedeId, c.Activo });

        builder.HasData(new ConfiguracionNegocio
        {
            ConfiguracionNegocioId = 1,
            SedeId = 1,
            TasaIGV = 0.08m,
            TasaIPM = 0.025m,
            FechaVigencia = new DateTime(2026, 1, 1),
            Activo = true
        });
    }
}
