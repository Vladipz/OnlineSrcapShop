using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProductCatalog.Api.Products;

namespace ProductCatalog.Api.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name).IsRequired().HasMaxLength(300);
        builder.Property(product => product.Description).HasMaxLength(5000);
        builder.Property(product => product.ImageUrl).IsRequired().HasMaxLength(2048);
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.CurrencyCode).HasMaxLength(3);
        builder.Property(product => product.SourceUrl).IsRequired().HasMaxLength(2048);
        builder.Property(product => product.CreatedAtUtc).IsRequired();
        builder.Property(product => product.UpdatedAtUtc).IsRequired();

        builder.HasIndex(product => product.SourceUrl).IsUnique();
    }
}
