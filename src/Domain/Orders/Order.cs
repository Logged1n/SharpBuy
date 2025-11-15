using Domain.Addresses;
using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Orders;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = new();
    private Order() { }

    public Guid Id { get;  private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? ShippingAddressId { get; private set; }
    public Guid? BillingAddressId { get; private set; }
    public Money Total { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order Create(Guid userId, Guid shippingAddressId, Guid? billingAddressId = null)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(shippingAddressId, Guid.Empty);
        if (billingAddressId.HasValue)
            ArgumentOutOfRangeException.ThrowIfEqual(billingAddressId.Value, Guid.Empty);

        return new Order()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Status = OrderStatus.Open,
            UserId = userId,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = billingAddressId ?? shippingAddressId,
            Total = Money.Zero("USD")
        };
    }

    public Result AddOrderItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Open)
            return Result.Failure(OrderErrors.OrderNotOpen);
        if (quantity <= 0)
            return Result.Failure(OrderErrors.InvalidQuantity);

        OrderItem? item = Items.FirstOrDefault(oi => oi.ProductId == productId);
        if (item != null)
            return Result.Failure(OrderErrors.ItemAlreadyExists(productId));

        _items.Add(OrderItem.Create(Id, productId, productName, unitPrice, quantity));
        RecalculateTotal();
        return Result.Success();
    }

    public Result MoveToStatus(OrderStatus status)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            return Result.Failure(OrderErrors.AlreadyFinished);

        Status = status;
        ModifiedAt = DateTime.UtcNow;

        if (status == OrderStatus.Completed)
            CompletedAt = DateTime.UtcNow;

        return Result.Success();
    }
    private void RecalculateTotal()
    {
        if (_items.Count == 0)
        {
            Total = Money.Zero(Total?.Currency ?? "USD");
            return;
        }

        Total = _items
            .Select(item => item.TotalPrice)
            .Aggregate((a, b) => a + b);
    }

}
