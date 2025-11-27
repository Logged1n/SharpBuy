using Domain.Carts;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<DomainUser>
{
    public void Configure(EntityTypeBuilder<DomainUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(9);

        builder.Property(u => u.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.PrimaryAddressId);

        builder.Ignore(u => u.FullName);

        builder.HasOne(u => u.Cart)
            .WithOne()
            .HasForeignKey<Cart>(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(u => u.Addresses)
            .WithOne()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.CreatedAt);

        builder.Ignore(u => u.DomainEvents);
    }
}
