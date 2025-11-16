using Application.Products.Add;
using Domain.Categories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Products;

public class AddProductCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AddProductCommandHandler _handler;

    public AddProductCommandHandlerTests()
    {
        _dbContext = Substitute.For<IApplicationDbContext>();
        _handler = new AddProductCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccessWithProductId()
    {
        // Arrange
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var categories = new List<Category>
        {
            Category.Create("Electronics"),
            Category.Create("Computers")
        };

        // Set the IDs via reflection since they're private setters
        typeof(Category).GetProperty("Id")!.SetValue(categories[0], categoryId1);
        typeof(Category).GetProperty("Id")!.SetValue(categories[1], categoryId2);

        var categoriesDbSet = DbSetMock.Create(categories);
        _dbContext.Categories.Returns(categoriesDbSet);
        _dbContext.Products.Returns(DbSetMock.Create<Product>());

        var command = new AddProductCommand(
            "Test Product",
            "Description",
            new SharedKernel.ValueObjects.Money(99.99m, "USD"),
            [categoryId1, categoryId2]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCategoryIds_ShouldReturnFailure()
    {
        // Arrange
        var validCategoryId = Guid.NewGuid();
        var invalidCategoryId = Guid.NewGuid();

        var categories = new List<Category>
        {
            Category.Create("Electronics")
        };

        typeof(Category).GetProperty("Id")!.SetValue(categories[0], validCategoryId);

        var categoriesDbSet = DbSetMock.Create(categories);
        _dbContext.Categories.Returns(categoriesDbSet);

        var command = new AddProductCommand(
            "Test Product",
            "Description",
            new SharedKernel.ValueObjects.Money(99.99m, "USD"),
            [validCategoryId, invalidCategoryId]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.InconsistentData);
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
