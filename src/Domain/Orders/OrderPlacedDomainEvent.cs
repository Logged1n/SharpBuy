using SharedKernel;

namespace Domain.Orders;

public sealed class OrderPlacedDomainEvent(Guid orderId, Guid userId) : IDomainEvent
{
    public Guid OrderId { get; } = orderId;
    public Guid UserId { get; } = userId;
}
