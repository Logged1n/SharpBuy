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
        var categories = new List<Category>
        {
            Category.Create("Electronics"),
            Category.Create("Computers")
        };

        DbSet<Category> categoriesDbSet = DbSetMock.Create(categories);
        _dbContext.Categories.Returns(categoriesDbSet);
        _dbContext.Products.Returns(x => DbSetMock.Create<Product>());

        var command = new AddProductCommand(
            "Test Product",
            "Description",
            10,
            new SharedKernel.ValueObjects.Money(99.99m, "USD"),
            [categories[0].Id, categories[0].Id],
            "test/path");

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        //await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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

        DbSet<Category> categoriesDbSet = DbSetMock.Create(categories);
        _dbContext.Categories.Returns(categoriesDbSet);

        var command = new AddProductCommand(
            "Test Product",
            "Description",
            -1,
            new SharedKernel.ValueObjects.Money(99.99m, "USD"),
            [validCategoryId, invalidCategoryId],
            string.Empty);

        // Act
        await _handler.Handle(command, CancellationToken.None).ShouldThrowAsync<ArgumentException>();

        //await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
