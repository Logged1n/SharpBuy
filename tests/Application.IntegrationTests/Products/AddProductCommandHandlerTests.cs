using Application.Abstractions.Caching;
using Application.Products.Add;
using Domain.Categories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using SharedKernel;
using SharedKernel.ValueObjects;
using Tests.Integration.Infrastructure;
using Xunit;

namespace Application.IntegrationTests.Products;

public class AddProductCommandHandlerTests : IntegrationTestBase
{
    [Fact]
    public async Task Handle_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var category1 = Category.Create("Electronics");
        var category2 = Category.Create("Computers");
        DbContext.Categories.AddRange(category1, category2);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);
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
        var category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);
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
        var category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);
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
        var category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);
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
        var category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);
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
        var category = Category.Create("Electronics");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        ICacheInvalidator mockCacheInvalidator = Substitute.For<ICacheInvalidator>();
        var handler = new AddProductCommandHandler(DbContext, mockCacheInvalidator);

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

        List<Product> products = await DbContext.Products.ToListAsync();
        products.Count.ShouldBe(2);
    }
}
