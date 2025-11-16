using Domain.Products;

namespace Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        string name = "Test Product";
        string description = "Test Description";
        int quantity = 10;
        decimal priceAmount = 99.99m;
        string priceCurrency = "USD";
        string mainPhotoPath = "/photos/main.jpg";

        // Act
        var product = Product.Create(
            name,
            description,
            quantity,
            priceAmount,
            priceCurrency,
            mainPhotoPath);

        // Assert
        product.ShouldNotBeNull();
        product.Id.ShouldNotBeSameAs(Guid.Empty);
        product.Name.ShouldBe(name);
        product.Description.ShouldBe(description);
        product.Price.Amount.ShouldBe(priceAmount);
        product.Price.Currency.ShouldBe(priceCurrency);
        product.MainPhotoPath.ShouldBe(mainPhotoPath);
        product.PhotoPaths.ShouldHaveSingleItem();
        product.PhotoPaths.ShouldContain(mainPhotoPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange & Act
        Action act = () => Product.Create(
            invalidName!,
            "Description",
            10,
            99.99m,
            "USD",
            "/photo.jpg");

        // Assert
        Should.Throw<ArgumentException>(act);
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
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddPhoto_ShouldAddPhotoToCollection()
    {
        // Arrange
        Product product = CreateValidProduct();
        string newPhotoPath = "/photos/additional.jpg";

        // Act
        product.AddPhoto(newPhotoPath);

        // Assert
        product.PhotoPaths.Count.ShouldNotBeSameAs(2);
        product.PhotoPaths.ShouldContain(newPhotoPath);
    }

    [Fact]
    public void AddToCategory_WithValidCategoryId_ShouldSucceed()
    {
        // Arrange
        Product product = CreateValidProduct();
        var categoryId = Guid.NewGuid();

        // Act
        Result result = product.AddToCategory(categoryId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Categories.ShouldHaveSingleItem();
        product.Categories.First().CategoryId.ShouldBe(categoryId);
    }

    [Fact]
    public void AddToCategory_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Product product = CreateValidProduct();

        // Act
        Action act = () => product.AddToCategory(Guid.Empty);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddToCategory_WhenAlreadyInCategory_ShouldReturnFailure()
    {
        // Arrange
        Product product = CreateValidProduct();
        var categoryId = Guid.NewGuid();
        product.AddToCategory(categoryId);

        // Act
        Result result = product.AddToCategory(categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.AlreadyInCategory(product.Id, categoryId));
    }

    [Fact]
    public void RemoveFromCategory_WithValidCategoryId_ShouldSucceed()
    {
        // Arrange
        Product product = CreateValidProduct();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        product.AddToCategory(categoryId1);
        product.AddToCategory(categoryId2);

        // Act
        Result result = product.RemoveFromCategory(categoryId1);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        product.Categories.ShouldHaveSingleItem();
        product.Categories.First().CategoryId.ShouldBe(categoryId2);
    }

    [Fact]
    public void RemoveFromCategory_WhenNotInCategory_ShouldReturnFailure()
    {
        // Arrange
        Product product = CreateValidProduct();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        product.AddToCategory(categoryId1);

        // Act
        Result result = product.RemoveFromCategory(categoryId2);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NotInCategory(product.Id, categoryId2));
    }

    [Fact]
    public void RemoveFromCategory_WhenOnlyOneCategory_ShouldReturnFailure()
    {
        // Arrange
        Product product = CreateValidProduct();
        var categoryId = Guid.NewGuid();
        product.AddToCategory(categoryId);

        // Act
        Result result = product.RemoveFromCategory(categoryId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ProductErrors.NoCategoriesAssigned);
        product.Categories.ShouldHaveSingleItem();
    }

    [Fact]
    public void RemoveFromCategory_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        Product product = CreateValidProduct();

        // Act
        Action act = () => product.RemoveFromCategory(Guid.Empty);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
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
