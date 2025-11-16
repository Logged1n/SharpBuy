using Domain.Products;
using Domain.Categories;
using Infrastructure.IntegrationTests;

namespace Infrastructure.IntegrationTests.Database;

public class ProductRepositoryTests : BaseIntegrationTest
{
    [Fact]
    public async Task AddProduct_ShouldPersistToDatabase()
    {
        // Arrange
        var product = Product.Create(
            "Test Product",
            "Test Description",
            10,
            99.99m,
            "USD",
            "/photos/test.jpg");

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            context.Products.Add(product);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            Product? savedProduct = await context.Products.FindAsync(product.Id);
            savedProduct.ShouldNotBeNull();
            savedProduct!.Name.ShouldBe("Test Product");
            savedProduct.Description.ShouldBe("Test Description");
            savedProduct.Price.Amount.ShouldBe(99.99m);
            savedProduct.Price.Currency.ShouldBe("USD");
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task AddProduct_WithCategories_ShouldPersistRelationships()
    {
        // Arrange
        var category1 = Category.Create("Electronics");
        var category2 = Category.Create("Computers");
        Guid productId = Guid.Empty;

        await ExecuteInTransactionAsync(async context =>
        {
            context.Categories.AddRange(category1, category2);
            return await context.SaveChangesAsync();
        });

        var product = Product.Create(
            "Laptop",
            "Gaming Laptop",
            5,
            1299.99m,
            "USD",
            "/photos/laptop.jpg");

        product.AddToCategory(category1.Id);
        product.AddToCategory(category2.Id);
        productId = product.Id;

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            context.Products.Add(product);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            Product? savedProduct = await context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == productId);

            savedProduct.ShouldNotBeNull();
            savedProduct!.Categories.Count.ShouldBe(2);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task UpdateProduct_ShouldPersistChanges()
    {
        // Arrange
        var product = Product.Create(
            "Original Name",
            "Original Description",
            10,
            99.99m,
            "USD",
            "/photos/test.jpg");

        await ExecuteInTransactionAsync(async context =>
        {
            context.Products.Add(product);
            return await context.SaveChangesAsync();
        });

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            Product? trackedProduct = await context.Products.FindAsync(product.Id);
            // Update via reflection since properties are private setters
            typeof(Product).GetProperty("Name")!.SetValue(trackedProduct, "Updated Name");
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            Product? updatedProduct = await context.Products.FindAsync(product.Id);
            updatedProduct!.Name.ShouldBe("Updated Name");
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task DeleteProduct_ShouldRemoveFromDatabase()
    {
        // Arrange
        var product = Product.Create(
            "Test Product",
            "Test Description",
            10,
            99.99m,
            "USD",
            "/photos/test.jpg");

        await ExecuteInTransactionAsync(async context =>
        {
            context.Products.Add(product);
            return await context.SaveChangesAsync();
        });

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            Product? trackedProduct = await context.Products.FindAsync(product.Id);
            context.Products.Remove(trackedProduct!);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            Product? deletedProduct = await context.Products.FindAsync(product.Id);
            deletedProduct.ShouldBeNull();
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task QueryProducts_WithFilter_ShouldReturnCorrectResults()
    {
        // Arrange
        var product1 = Product.Create("Product A", "Desc", 10, 50m, "USD", "/photo.jpg");
        var product2 = Product.Create("Product B", "Desc", 10, 100m, "USD", "/photo.jpg");
        var product3 = Product.Create("Product C", "Desc", 10, 150m, "USD", "/photo.jpg");

        await ExecuteInTransactionAsync(async context =>
        {
            context.Products.AddRange(product1, product2, product3);
            return await context.SaveChangesAsync();
        });

        // Act & Assert
        await ExecuteInTransactionAsync(async context =>
        {
            List<Product> expensiveProducts = await context.Products
                .Where(p => p.Price.Amount > 75m)
                .ToListAsync();

            expensiveProducts.Count.ShouldBe(2);
            expensiveProducts.ShouldContain(p => p.Name == "Product B");
            expensiveProducts.ShouldContain(p => p.Name == "Product C");
            return Task.CompletedTask;
        });
    }
}
