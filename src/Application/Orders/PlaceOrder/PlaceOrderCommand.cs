using Application.Abstractions.Messaging;
using Domain.Addresses;
using SharedKernel.Dtos;

namespace Application.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid UserId,
    Guid? ShippingAddressId,
    Guid? BillingAddressId,
    AddressDto? ShippingAddress,
    AddressDto? BillingAddress,
    string PaymentIntentId) : ICommand<Guid>;
