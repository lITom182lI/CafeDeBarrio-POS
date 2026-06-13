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

        // Índice de búsqueda rápida por sede+activo
        builder.HasIndex(c => new { c.SedeId, c.Activo })
               .HasDatabaseName("IX_ConfiguracionNegocio_SedeId_Activo");

        // Garantía de esquema: solo una configuración activa por sede
        builder.HasIndex(c => c.SedeId)
               .HasFilter("[Activo] = 1")
               .IsUnique()
               .HasDatabaseName("UX_ConfiguracionNegocio_SedeId_Activa");
    }
}
