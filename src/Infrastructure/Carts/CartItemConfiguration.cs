using Domain.Carts;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Carts;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => new { ci.CartId, ci.ProductId });

        builder.Property(ci => ci.CartId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .IsRequired();

        builder.Property(ci => ci.AddedAt)
            .IsRequired();

        builder.Property(ci => ci.UpdatedAt);

        builder.HasOne<Cart>()
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ci => ci.ProductId);
        builder.HasIndex(ci => ci.AddedAt);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_CartItem_Quantity", "\"Quantity\" > 0"));
    }
}
