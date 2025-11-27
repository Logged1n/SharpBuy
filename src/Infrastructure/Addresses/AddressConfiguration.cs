using Domain.Addresses;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Addresses;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Line1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Line2)
            .HasMaxLength(200); // nullable

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne<DomainUser>()
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}
