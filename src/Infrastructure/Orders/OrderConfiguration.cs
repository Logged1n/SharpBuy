using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;
internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasMany(o => o.Items)
          .WithOne(li => li.Parent)
          .HasForeignKey(li => li.ParentId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
