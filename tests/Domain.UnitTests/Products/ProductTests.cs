using Domain.Products;

namespace Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var quantity = 10;
        var priceAmount = 99.99m;
        var priceCurrency = "USD";
        var mainPhotoPath = "/photos/main.jpg";

        // Act
        var product = Product.Create(
            name,
            description,
            quantity,
            priceAmount,
            priceCurrency,
            mainPhotoPath);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Amount.Should().Be(priceAmount);
        product.Price.Currency.Should().Be(priceCurrency);
        product.MainPhotoPath.Should().Be(mainPhotoPath);
        product.PhotoPaths.Should().ContainSingle();
        product.PhotoPaths.Should().Contain(mainPhotoPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange & Act
        Action act = () => Product.Create(
            invalidName,
            "Description",
            10,
            99.99m,
            "USD",
            "/photo.jpg");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidQuantity_ShouldThrowArgumentOutOfRangeException(int invalidQuantity)
    {
        // Arrange & Act
        Action act = () => Product.Create(
            "Product",
            "Description",
            invalidQuantity,
            99.99m,
            "USD",
            "/photo.jpg");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddPhoto_ShouldAddPhotoToCollection()
    {
        // Arrange
        var product = CreateValidProduct();
        var newPhotoPath = "/photos/additional.jpg";

        // Act
        product.AddPhoto(newPhotoPath);

        // Assert
        product.PhotoPaths.Should().HaveCount(2);
        product.PhotoPaths.Should().Contain(newPhotoPath);
    }

    [Fact]
    public void AddToCategory_WithValidCategoryId_ShouldSucceed()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId = Guid.NewGuid();

        // Act
        var result = product.AddToCategory(categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Categories.Should().ContainSingle();
        product.Categories.First().CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void AddToCategory_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        Action act = () => product.AddToCategory(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddToCategory_WhenAlreadyInCategory_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId = Guid.NewGuid();
        product.AddToCategory(categoryId);

        // Act
        var result = product.AddToCategory(categoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.AlreadyInCategory(product.Id, categoryId));
    }

    [Fact]
    public void RemoveFromCategory_WithValidCategoryId_ShouldSucceed()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        product.AddToCategory(categoryId1);
        product.AddToCategory(categoryId2);

        // Act
        var result = product.RemoveFromCategory(categoryId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Categories.Should().ContainSingle();
        product.Categories.First().CategoryId.Should().Be(categoryId2);
    }

    [Fact]
    public void RemoveFromCategory_WhenNotInCategory_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        product.AddToCategory(categoryId1);

        // Act
        var result = product.RemoveFromCategory(categoryId2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NotInCategory(product.Id, categoryId2));
    }

    [Fact]
    public void RemoveFromCategory_WhenOnlyOneCategory_ShouldReturnFailure()
    {
        // Arrange
        var product = CreateValidProduct();
        var categoryId = Guid.NewGuid();
        product.AddToCategory(categoryId);

        // Act
        var result = product.RemoveFromCategory(categoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NoCategoriesAssigned);
        product.Categories.Should().ContainSingle();
    }

    [Fact]
    public void RemoveFromCategory_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var product = CreateValidProduct();

        // Act
        Action act = () => product.RemoveFromCategory(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static Product CreateValidProduct()
    {
        return Product.Create(
            "Test Product",
            "Test Description",
            10,
            99.99m,
            "USD",
            "/photos/main.jpg");
    }
}
