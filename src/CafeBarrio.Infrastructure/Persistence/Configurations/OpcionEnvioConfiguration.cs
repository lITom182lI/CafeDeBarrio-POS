using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class OpcionEnvioConfiguration : IEntityTypeConfiguration<OpcionEnvio>
{
    public void Configure(EntityTypeBuilder<OpcionEnvio> builder)
    {
        builder.ToTable("OpcionEnvio");
        builder.HasKey(x => x.OpcionEnvioId);
        builder.Property(x => x.OpcionEnvioId).HasColumnName("opcion_envio_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        builder.Property(x => x.Tarifa).HasColumnName("tarifa").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
    }
}
