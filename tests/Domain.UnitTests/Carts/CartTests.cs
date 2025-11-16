using Domain.Carts;

namespace Domain.UnitTests.Carts;

public class CartTests
{
    [Fact]
    public void Create_WithValidOwnerId_ShouldCreateCart()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var cart = Cart.Create(ownerId);

        // Assert
        cart.Should().NotBeNull();
        cart.OwnerId.Should().Be(ownerId);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        Action act = () => Cart.Create(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AddCartItem_WithValidData_ShouldSucceed()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        var quantity = 2;

        // Act
        var result = cart.AddCartItem(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().ContainSingle();
        cart.Items.First().ProductId.Should().Be(productId);
        cart.Items.First().Quantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddCartItem_WithInvalidQuantity_ShouldReturnFailure(int invalidQuantity)
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");

        // Act
        var result = cart.AddCartItem(productId, unitPrice, invalidQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartItemErrors.InvalidQuantity);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddCartItem_WhenItemAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        var result = cart.AddCartItem(productId, unitPrice, 2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartErrors.ItemAlreadyExists(productId));
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void RemoveCartItem_WithValidProductId_ShouldSucceed()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        var result = cart.RemoveCartItem(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCartItem_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();

        // Act
        var result = cart.RemoveCartItem(productId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartErrors.ItemNotFound(productId));
    }

    [Fact]
    public void ChangeItemQuantity_WithValidData_ShouldSucceed()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        var result = cart.ChnageItemQuantity(productId, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        cart.Items.First().Quantity.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeItemQuantity_WithInvalidQuantity_ShouldReturnFailure(int invalidQuantity)
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        var result = cart.ChnageItemQuantity(productId, invalidQuantity);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartItemErrors.InvalidQuantity);
    }

    [Fact]
    public void ChangeItemQuantity_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        var cart = CreateValidCart();
        var productId = Guid.NewGuid();

        // Act
        var result = cart.ChnageItemQuantity(productId, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CartErrors.ItemNotFound(productId));
    }

    [Fact]
    public void Total_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        var cart = CreateValidCart();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var price1 = new Money(10.00m, "USD");
        var price2 = new Money(20.00m, "USD");

        cart.AddCartItem(product1Id, price1, 2); // 20.00
        cart.AddCartItem(product2Id, price2, 3); // 60.00

        // Act
        var total = cart.Total;

        // Assert
        total.Amount.Should().Be(80.00m);
        total.Currency.Should().Be("USD");
    }

    private static Cart CreateValidCart()
    {
        return Cart.Create(Guid.NewGuid());
    }
}
