using SharedKernel;

namespace Domain.Orders;

public sealed class OrderStatusChangedDomainEvent(
    Guid orderId,
    Guid userId,
    OrderStatus oldStatus,
    OrderStatus newStatus) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
    public Guid UserId { get; } = userId;
    public OrderStatus OldStatus { get; } = oldStatus;
    public OrderStatus NewStatus { get; } = newStatus;
}
