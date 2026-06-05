using CafeBarrio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafeBarrio.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Producto");
        builder.HasKey(x => x.ProductoId);
        builder.Property(x => x.ProductoId).HasColumnName("producto_id");
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        builder.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("descripcion");
        builder.Property(x => x.Costo).HasColumnName("costo").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Precio).HasColumnName("precio").HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CantidadDisponible).HasColumnName("cantidad_disponible").IsRequired();
        builder.Property(x => x.CantidadPorUnidad).HasColumnName("cantidad_por_unidad").HasMaxLength(50);
        builder.Property(x => x.ImagenUrl).HasColumnName("imagen_url").HasMaxLength(500);
        builder.Property(x => x.EsMayorista).HasColumnName("es_mayorista").IsRequired();
        builder.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        builder.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion").IsRequired();
        builder.Property(x => x.FechaActualizacion).HasColumnName("fecha_actualizacion").IsRequired();

        builder.HasOne(x => x.Categoria)
               .WithMany(c => c.Productos)
               .HasForeignKey(x => x.CategoriaId);
    }
}
