using Domain.Products;
using Domain.Categories;

namespace Infrastructure.IntegrationTests.Database;

public class ProductRepositoryTests : BaseIntegrationTest
{
    public ProductRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

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
        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();

        // Clear tracked entities
        DbContext.ChangeTracker.Clear();

        // Assert
        var savedProduct = await DbContext.Products.FindAsync(product.Id);
        savedProduct.ShouldNotBeNull();
        savedProduct!.Name.ShouldBe("Test Product");
        savedProduct.Description.ShouldBe("Test Description");
        savedProduct.Price.Amount.ShouldBe(99.99m);
        savedProduct.Price.Currency.ShouldBe("USD");
    }

    [Fact]
    public async Task AddProduct_WithCategories_ShouldPersistRelationships()
    {
        // Arrange
        var category1 = Category.Create("Electronics");
        var category2 = Category.Create("Computers");

        DbContext.Categories.AddRange(category1, category2);
        await DbContext.SaveChangesAsync();

        var product = Product.Create(
            "Laptop",
            "Gaming Laptop",
            5,
            1299.99m,
            "USD",
            "/photos/laptop.jpg");

        product.AddToCategory(category1.Id);
        product.AddToCategory(category2.Id);

        // Act
        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var savedProduct = await DbContext.Products
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        savedProduct.ShouldNotBeNull();
        savedProduct!.Categories.ShouldHaveCount(2);
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

        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var trackedProduct = await DbContext.Products.FindAsync(product.Id);
        // Update via reflection since properties are private setters
        typeof(Product).GetProperty("Name")!.SetValue(trackedProduct, "Updated Name");
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var updatedProduct = await DbContext.Products.FindAsync(product.Id);
        updatedProduct!.Name.ShouldBe("Updated Name");
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

        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var trackedProduct = await DbContext.Products.FindAsync(product.Id);
        DbContext.Products.Remove(trackedProduct!);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedProduct = await DbContext.Products.FindAsync(product.Id);
        deletedProduct.ShouldBeNull();
    }

    [Fact]
    public async Task QueryProducts_WithFilter_ShouldReturnCorrectResults()
    {
        // Arrange
        var product1 = Product.Create("Product A", "Desc", 10, 50m, "USD", "/photo.jpg");
        var product2 = Product.Create("Product B", "Desc", 10, 100m, "USD", "/photo.jpg");
        var product3 = Product.Create("Product C", "Desc", 10, 150m, "USD", "/photo.jpg");

        DbContext.Products.AddRange(product1, product2, product3);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var expensiveProducts = await DbContext.Products
            .Where(p => p.Price.Amount > 75m)
            .ToListAsync();

        // Assert
        expensiveProducts.ShouldHaveCount(2);
        expensiveProducts.ShouldContain(p => p.Name == "Product B");
        expensiveProducts.ShouldContain(p => p.Name == "Product C");
    }
}
