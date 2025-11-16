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
        cart.ShouldNotBeNull();
        cart.OwnerId.ShouldBe(ownerId);
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldThrowArgumentOutOfRangeException()
    {
        // Act
        Action act = () => Cart.Create(Guid.Empty);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddCartItem_WithValidData_ShouldSucceed()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        int quantity = 2;

        // Act
        Result result = cart.AddCartItem(productId, unitPrice, quantity);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cart.Items.ShouldHaveSingleItem();
        cart.Items.First().ProductId.ShouldBe(productId);
        cart.Items.First().Quantity.ShouldBe(quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddCartItem_WithInvalidQuantity_ShouldReturnFailure(int invalidQuantity)
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");

        // Act
        Result result = cart.AddCartItem(productId, unitPrice, invalidQuantity);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CartItemErrors.InvalidQuantity);
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddCartItem_WhenItemAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        Result result = cart.AddCartItem(productId, unitPrice, 2);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CartErrors.ItemAlreadyExists(productId));
        cart.Items.ShouldHaveSingleItem();
    }

    [Fact]
    public void RemoveCartItem_WithValidProductId_ShouldSucceed()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        Result result = cart.RemoveCartItem(productId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveCartItem_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();

        // Act
        Result result = cart.RemoveCartItem(productId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CartErrors.ItemNotFound(productId));
    }

    [Fact]
    public void ChangeItemQuantity_WithValidData_ShouldSucceed()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        Result result = cart.ChnageItemQuantity(productId, 5);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        cart.Items.First().Quantity.ShouldBe(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeItemQuantity_WithInvalidQuantity_ShouldReturnFailure(int invalidQuantity)
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(29.99m, "USD");
        cart.AddCartItem(productId, unitPrice, 1);

        // Act
        Result result = cart.ChnageItemQuantity(productId, invalidQuantity);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CartItemErrors.InvalidQuantity);
    }

    [Fact]
    public void ChangeItemQuantity_WhenItemNotFound_ShouldReturnFailure()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var productId = Guid.NewGuid();

        // Act
        Result result = cart.ChnageItemQuantity(productId, 5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(CartErrors.ItemNotFound(productId));
    }

    [Fact]
    public void Total_WithMultipleItems_ShouldCalculateCorrectly()
    {
        // Arrange
        Cart cart = CreateValidCart();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var price1 = new Money(10.00m, "USD");
        var price2 = new Money(20.00m, "USD");

        cart.AddCartItem(product1Id, price1, 2); // 20.00
        cart.AddCartItem(product2Id, price2, 3); // 60.00

        // Act
        Money total = cart.Total;

        // Assert
        total.Amount.ShouldBe(80.00m);
        total.Currency.ShouldBe("USD");
    }

    private static Cart CreateValidCart()
    {
        return Cart.Create(Guid.NewGuid());
    }
}
