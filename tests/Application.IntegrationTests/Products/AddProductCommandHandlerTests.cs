using Application.Products.Add;
using Domain.Categories;
using Domain.Products;
using SharedKernel.ValueObjects;

namespace Application.IntegrationTests.Products;

public class AddProductCommandHandlerTests : BaseIntegrationTest
{
    public AddProductCommandHandlerTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        Category category1 = Category.Create("Electronics");
        Category category2 = Category.Create("Computers");
        DbContext.Categories.AddRange(category1, category2);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);
        var command = new AddProductCommand(
            "Gaming Laptop",
            "High-performance gaming laptop",
            10,
            new Money(1299.99m, "USD"),
            [category1.Id, category2.Id],
            "/photos/laptop.jpg");

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        // Verify product was created in database
        Product? product = await DbContext.Products.FindAsync(result.Value);
        product.ShouldNotBeNull();
        product.Name.ShouldBe("Gaming Laptop");
        product.Description.ShouldBe("High-performance gaming laptop");
        product.Price.Amount.ShouldBe(1299.99m);
        product.Price.Currency.ShouldBe("USD");
        product.MainPhotoPath.ShouldBe("/photos/laptop.jpg");
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        Category category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);
        var command = new AddProductCommand(
            "",
            "Description",
            10,
            new Money(99.99m, "USD"),
            [category.Id],
            "/photo.jpg");

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Category category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);
        var command = new AddProductCommand(
            "Product",
            "Description",
            -1,
            new Money(99.99m, "USD"),
            [category.Id],
            "/photo.jpg");

        // Act & Assert
        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithZeroQuantity_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Category category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);
        var command = new AddProductCommand(
            "Product",
            "Description",
            0,
            new Money(99.99m, "USD"),
            [category.Id],
            "/photo.jpg");

        // Act & Assert
        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithEmptyPhotoPath_ShouldThrowArgumentException()
    {
        // Arrange
        Category category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);
        var command = new AddProductCommand(
            "Product",
            "Description",
            10,
            new Money(99.99m, "USD"),
            [category.Id],
            "");

        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(async () =>
            await handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MultipleCalls_ShouldCreateMultipleProducts()
    {
        // Arrange
        Category category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var handler = new AddProductCommandHandler(DbContext);

        // Act
        Result<Guid> result1 = await handler.Handle(new AddProductCommand(
            "Product 1",
            "Description 1",
            10,
            new Money(99.99m, "USD"),
            [category.Id],
            "/photo1.jpg"), CancellationToken.None);

        Result<Guid> result2 = await handler.Handle(new AddProductCommand(
            "Product 2",
            "Description 2",
            5,
            new Money(49.99m, "EUR"),
            [category.Id],
            "/photo2.jpg"), CancellationToken.None);

        // Assert
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result1.Value.ShouldNotBe(result2.Value);

        List<Product> products = DbContext.Products.ToList();
        products.Count.ShouldBe(2);
    }
}
