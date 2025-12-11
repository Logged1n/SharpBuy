using Domain.Categories;
using Domain.Products;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        SeedData(dbContext);
    }

    private static void SeedData(ApplicationDbContext dbContext)
    {
        if (dbContext.Categories.Any() || dbContext.Products.Any())
        {
            return;
        }

        // Create 5 categories
        Category[] categories =
        [
            Category.Create("Electronics"),
            Category.Create("Clothing"),
            Category.Create("Books"),
            Category.Create("Home & Garden"),
            Category.Create("Sports & Outdoors")
        ];

        dbContext.Categories.AddRange(categories);
        dbContext.SaveChanges();

        // Create 20 products
        Product[] products =
        [
            // Electronics (4 products)
            Product.Create("Laptop Dell XPS 15", "High-performance laptop with 16GB RAM and 512GB SSD", 25, 1299.99m, "USD", "/images/laptop1.jpg"),
            Product.Create("Wireless Mouse Logitech", "Ergonomic wireless mouse with precision tracking", 100, 29.99m, "USD", "/images/mouse1.jpg"),
            Product.Create("USB-C Hub", "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader", 50, 49.99m, "USD", "/images/hub1.jpg"),
            Product.Create("Bluetooth Headphones", "Noise-canceling over-ear headphones with 30-hour battery", 40, 199.99m, "USD", "/images/headphones1.jpg"),

            // Clothing (4 products)
            Product.Create("Men's Cotton T-Shirt", "Comfortable cotton t-shirt in various colors", 200, 19.99m, "USD", "/images/tshirt1.jpg"),
            Product.Create("Women's Jeans", "Classic fit jeans with stretch fabric", 150, 59.99m, "USD", "/images/jeans1.jpg"),
            Product.Create("Winter Jacket", "Waterproof winter jacket with thermal insulation", 75, 129.99m, "USD", "/images/jacket1.jpg"),
            Product.Create("Running Shoes", "Lightweight running shoes with cushioned sole", 120, 89.99m, "USD", "/images/shoes1.jpg"),

            // Books (4 products)
            Product.Create("Clean Architecture", "Robert C. Martin's guide to software architecture", 80, 39.99m, "USD", "/images/book1.jpg"),
            Product.Create("Domain-Driven Design", "Eric Evans' classic on DDD principles", 60, 49.99m, "USD", "/images/book2.jpg"),
            Product.Create("The Pragmatic Programmer", "Your journey to mastery in software development", 90, 44.99m, "USD", "/images/book3.jpg"),
            Product.Create("Design Patterns", "Gang of Four's essential patterns reference", 70, 54.99m, "USD", "/images/book4.jpg"),

            // Home & Garden (4 products)
            Product.Create("Coffee Maker", "Programmable 12-cup coffee maker with timer", 45, 79.99m, "USD", "/images/coffee1.jpg"),
            Product.Create("Indoor Plant Pot", "Ceramic pot with drainage for small plants", 300, 14.99m, "USD", "/images/pot1.jpg"),
            Product.Create("LED Desk Lamp", "Adjustable LED lamp with touch control", 85, 34.99m, "USD", "/images/lamp1.jpg"),
            Product.Create("Throw Pillow Set", "Set of 2 decorative throw pillows", 110, 24.99m, "USD", "/images/pillow1.jpg"),

            // Sports & Outdoors (4 products)
            Product.Create("Yoga Mat", "Non-slip yoga mat with carrying strap", 200, 29.99m, "USD", "/images/yoga1.jpg"),
            Product.Create("Camping Tent", "4-person waterproof camping tent", 30, 159.99m, "USD", "/images/tent1.jpg"),
            Product.Create("Water Bottle", "Insulated stainless steel water bottle 32oz", 180, 24.99m, "USD", "/images/bottle1.jpg"),
            Product.Create("Resistance Bands Set", "Set of 5 resistance bands for strength training", 95, 19.99m, "USD", "/images/bands1.jpg")
        ];

        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();

        // Assign products to categories
        List<Product> productList = products.ToList();

        // Electronics
        for (int i = 0; i < 4; i++)
        {
            productList[i].AddToCategory(categories[0].Id);
        }

        // Clothing
        for (int i = 4; i < 8; i++)
        {
            productList[i].AddToCategory(categories[1].Id);
        }

        // Books
        for (int i = 8; i < 12; i++)
        {
            productList[i].AddToCategory(categories[2].Id);
        }

        // Home & Garden
        for (int i = 12; i < 16; i++)
        {
            productList[i].AddToCategory(categories[3].Id);
        }

        // Sports & Outdoors
        for (int i = 16; i < 20; i++)
        {
            productList[i].AddToCategory(categories[4].Id);
        }

        dbContext.SaveChanges();
    }
}
