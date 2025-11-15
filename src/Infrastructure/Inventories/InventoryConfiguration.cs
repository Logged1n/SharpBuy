using Domain.Inventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Inventories;

internal sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.LastUpdated)
            .IsRequired();

        builder.HasIndex(i => i.ProductId);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Inventory_Quantity", "[Quantity] >= 0");
            t.HasCheckConstraint("CK_Inventory_ReservedQuantity", "[ReservedQuantity] >= 0");
            t.HasCheckConstraint("CK_Inventory_Reserved_Not_Greater_Than_Quantity",
                "[ReservedQuantity] <= [Quantity]");
        });
    }
}
