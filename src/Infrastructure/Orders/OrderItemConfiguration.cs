using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => new { oi.ParentId, oi.ProductId });
        builder.Property(oi => oi.ParentId).HasColumnName("OrderId");
        builder.Property(oi => oi.ProductId).HasColumnName("ProductId");

        builder.HasOne(oi => oi.Parent)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.ParentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .IsRequired();
    }
}
