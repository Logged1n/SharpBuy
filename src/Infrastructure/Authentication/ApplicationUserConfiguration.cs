using Application.Abstractions.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Authentication;
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.DomainUserId)
            .IsRequired();

        builder.HasOne(u => u.DomainUser)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.DomainUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.DomainUserId).IsUnique();
    }
}
