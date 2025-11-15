using Domain.Carts;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Carts;
internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.OwnerId);

        builder.Navigation(c => c.Items)
           .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<User>()
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
