using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Carts;
internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.HasMany(c => c.Items)
            .WithOne(li => li.Parent)
            .HasForeignKey(li => li.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
