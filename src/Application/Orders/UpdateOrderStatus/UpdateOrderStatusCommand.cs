using Application.Abstractions.Messaging;
using Domain.Orders;

namespace Application.Orders.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus) : ICommand;
