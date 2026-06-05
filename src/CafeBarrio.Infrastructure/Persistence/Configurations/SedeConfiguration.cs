using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class SedeConfiguration : IEntityTypeConfiguration<Sede>
{
    public void Configure(EntityTypeBuilder<Sede> builder)
    {
        builder.ToTable("Sede");
        builder.HasKey(x => x.SedeId);
        builder.Property(x => x.SedeId).HasColumnName("sede_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Distrito).HasColumnName("distrito").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ciudad).HasColumnName("ciudad").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
        builder.Property(x => x.EsPrincipal).HasColumnName("es_principal").IsRequired();
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        builder.Property(x => x.FechaApertura).HasColumnName("fecha_apertura");
    }
}
