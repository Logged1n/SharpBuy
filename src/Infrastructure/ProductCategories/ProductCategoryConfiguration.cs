using Domain.ProductCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.ProductCategories;

internal sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(pc => new { pc.ProductId, pc.CategoryId });

        builder.Property(pc => pc.ProductId)
            .IsRequired();

        builder.Property(pc => pc.CategoryId)
            .IsRequired();

        builder.HasIndex(pc => pc.ProductId);
        builder.HasIndex(pc => pc.CategoryId);
    }
}
