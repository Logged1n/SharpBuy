using Application.Abstractions.Messaging;
using Domain.Orders;
using SharedKernel;
using SharedKernel.Dtos;

namespace Application.Orders.GetAll;

public sealed record GetAllOrdersQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<OrderListItem>>;

public sealed record OrderListItem(
    Guid Id,
    Guid UserId,
    string UserFirstName,
    string UserLastName,
    string UserEmail,
    DateTime CreatedAt,
    DateTime ModifiedAt,
    DateTime? CompletedAt,
    OrderStatus Status,
    Guid? ShippingAddressId,
    Guid? BillingAddressId,
    AddressDto? ShippingAddress,
    AddressDto? BillingAddress,
    decimal TotalAmount,
    string TotalCurrency,
    ICollection<OrderItemDto> Items);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPriceAmount,
    string UnitPriceCurrency,
    int Quantity,
    decimal TotalPriceAmount,
    string TotalPriceCurrency);
