using Domain.Categories;
using Domain.Inventories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        builder.Property(p => p.MainPhotoPath)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(p => p.InventoryId)
            .IsRequired();
        
        builder.HasOne(p => p.Inventory)
            .WithOne()
            .HasForeignKey<Product>(p => p.InventoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        
        builder.Navigation(p => p.Inventory)
            .AutoInclude();
        
        builder.HasMany(p => p.Categories)
            .WithOne()
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Navigation(p => p.Categories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.InventoryId).IsUnique();
    }
}
