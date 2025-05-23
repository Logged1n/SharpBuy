using Domain.Addresses;
using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Orders;

public sealed class Order : Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public User User { get; set; }
    public Address? ShippingAddress { get; set; }
    public Address BillingAddress { get; set; }
    public ICollection<OrderItem> Items { get; set; }
    public Money Total => GetTotalValue();

    private Money GetTotalValue()
    {
        return new Money(Items.Sum(
            oi => oi.Quantity * oi.Product.Price.Amount),
            Items.First().Product.Price.Currency);
    }
}
