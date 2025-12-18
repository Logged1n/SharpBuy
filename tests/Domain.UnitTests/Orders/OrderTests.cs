using Domain.Orders;
using SharedKernel.ValueObjects;
using Shouldly;

namespace Domain.UnitTests.Orders;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();
        var billingAddressId = Guid.NewGuid();

        // Act
        var order = Order.Create(userId, shippingAddressId, billingAddressId);

        // Assert
        order.ShouldNotBeNull();
        order.Id.ShouldNotBe(Guid.Empty);
        order.UserId.ShouldBe(userId);
        order.ShippingAddressId.ShouldBe(shippingAddressId);
        order.BillingAddressId.ShouldBe(billingAddressId);
        order.Status.ShouldBe(OrderStatus.Open);
        order.Total.Amount.ShouldBe(0);
        order.Total.Currency.ShouldBe("USD");
        order.Items.ShouldBeEmpty();
        order.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
        order.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_WithoutBillingAddress_ShouldUseShippingAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();

        // Act
        var order = Order.Create(userId, shippingAddressId);

        // Assert
        order.BillingAddressId.ShouldBe(shippingAddressId);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var shippingAddressId = Guid.NewGuid();

        // Act
        Action act = () => Order.Create(Guid.Empty, shippingAddressId);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithEmptyShippingAddressId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Action act = () => Order.Create(userId, Guid.Empty);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithEmptyBillingAddressId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shippingAddressId = Guid.NewGuid();

        // Act
        Action act = () => Order.Create(userId, shippingAddressId, Guid.Empty);

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void AddOrderItem_WithValidData_ShouldAddItem()
    {
        // Arrange
        Order order = CreateValidOrder();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(10.99m, "USD");

        // Act
        Result result = order.AddOrderItem(productId, "Test Product", unitPrice, 2);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Items.ShouldHaveSingleItem();
        order.Items.First().ProductId.ShouldBe(productId);
        order.Items.First().ProductName.ShouldBe("Test Product");
        order.Items.First().UnitPrice.ShouldBe(unitPrice);
        order.Items.First().Quantity.ShouldBe(2);
        order.Total.Amount.ShouldBe(21.98m);
    }

    [Fact]
    public void AddOrderItem_WithMultipleItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        Order order = CreateValidOrder();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var unitPrice1 = new Money(10.00m, "USD");
        var unitPrice2 = new Money(15.00m, "USD");

        // Act
        order.AddOrderItem(product1Id, "Product 1", unitPrice1, 2);
        order.AddOrderItem(product2Id, "Product 2", unitPrice2, 3);

        // Assert
        order.Items.Count.ShouldBe(2);
        order.Total.Amount.ShouldBe(65.00m); // (10 * 2) + (15 * 3) = 65
        order.Total.Currency.ShouldBe("USD");
    }

    [Fact]
    public void AddOrderItem_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(10.99m, "USD");

        // Act
        Result result = order.AddOrderItem(productId, "Test Product", unitPrice, 0);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.InvalidQuantity);
        order.Items.ShouldBeEmpty();
    }

    [Fact]
    public void AddOrderItem_WithNegativeQuantity_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(10.99m, "USD");

        // Act
        Result result = order.AddOrderItem(productId, "Test Product", unitPrice, -5);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.InvalidQuantity);
    }

    [Fact]
    public void AddOrderItem_WhenItemAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        var productId = Guid.NewGuid();
        var unitPrice = new Money(10.99m, "USD");
        order.AddOrderItem(productId, "Test Product", unitPrice, 2);

        // Act
        Result result = order.AddOrderItem(productId, "Test Product", unitPrice, 1);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.ItemAlreadyExists(productId));
        order.Items.ShouldHaveSingleItem();
    }

    [Fact]
    public void AddOrderItem_WhenOrderNotOpen_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.MoveToStatus(OrderStatus.Confirmed);
        var productId = Guid.NewGuid();
        var unitPrice = new Money(10.99m, "USD");

        // Act
        Result result = order.AddOrderItem(productId, "Test Product", unitPrice, 2);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.OrderNotOpen);
    }

    [Fact]
    public void MoveToStatus_FromOpenToConfirmed_ShouldSucceed()
    {
        // Arrange
        Order order = CreateValidOrder();

        // Act
        Result result = order.MoveToStatus(OrderStatus.Confirmed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Confirmed);
        order.ModifiedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
        order.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void MoveToStatus_ToCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.MoveToStatus(OrderStatus.Confirmed);

        // Act
        Result result = order.MoveToStatus(OrderStatus.Completed);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        order.Status.ShouldBe(OrderStatus.Completed);
        order.CompletedAt.ShouldNotBeNull();
        order.CompletedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }

    [Fact]
    public void MoveToStatus_WhenAlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.MoveToStatus(OrderStatus.Confirmed);
        order.MoveToStatus(OrderStatus.Completed);

        // Act
        Result result = order.MoveToStatus(OrderStatus.Cancelled);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.AlreadyFinished);
        order.Status.ShouldBe(OrderStatus.Completed);
    }

    [Fact]
    public void MoveToStatus_WhenAlreadyCancelled_ShouldReturnFailure()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.MoveToStatus(OrderStatus.Cancelled);

        // Act
        Result result = order.MoveToStatus(OrderStatus.Confirmed);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(OrderErrors.AlreadyFinished);
        order.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public void MoveToStatus_ToConfirmed_ShouldRaiseOrderPlacedEvent()
    {
        // Arrange
        Order order = CreateValidOrder();

        // Act
        order.MoveToStatus(OrderStatus.Confirmed);

        // Assert
        OrderPlacedDomainEvent? orderPlacedEvent = order.DomainEvents.OfType<OrderPlacedDomainEvent>().FirstOrDefault();
        orderPlacedEvent.ShouldNotBeNull();
        orderPlacedEvent.OrderId.ShouldBe(order.Id);
        orderPlacedEvent.UserId.ShouldBe(order.UserId);
    }

    [Fact]
    public void MoveToStatus_NotToConfirmedFromOpen_ShouldRaiseOrderStatusChangedEvent()
    {
        // Arrange
        Order order = CreateValidOrder();
        order.MoveToStatus(OrderStatus.Confirmed);

        // Act
        order.MoveToStatus(OrderStatus.Completed);

        // Assert
        OrderStatusChangedDomainEvent? statusChangedEvent = order.DomainEvents.OfType<OrderStatusChangedDomainEvent>().FirstOrDefault();
        statusChangedEvent.ShouldNotBeNull();
        statusChangedEvent.OrderId.ShouldBe(order.Id);
        statusChangedEvent.OldStatus.ShouldBe(OrderStatus.Confirmed);
        statusChangedEvent.NewStatus.ShouldBe(OrderStatus.Completed);
    }

    private static Order CreateValidOrder()
    {
        return Order.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}
