using Application.Abstractions.Messaging;
using Application.Orders.GetAll;

namespace Application.Orders.GetById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderListItem>;
