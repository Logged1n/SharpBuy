using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Carts;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(ci => new { ci.ParentId, ci.ProductId });
        builder.Property(ci => ci.ParentId).HasColumnName("CartId");
        builder.Property(ci => ci.ProductId).HasColumnName("ProductId");

        builder.HasOne(ci => ci.Parent)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.ParentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .IsRequired();
    }
}
