using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class CategoriaCafeConfiguration : IEntityTypeConfiguration<CategoriaCafe>
{
    public void Configure(EntityTypeBuilder<CategoriaCafe> builder)
    {
        builder.ToTable("CategoriaCafe");
        builder.HasKey(x => x.CategoriaId);
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        builder.Property(x => x.Activa).HasColumnName("activa").IsRequired();
    }
}
