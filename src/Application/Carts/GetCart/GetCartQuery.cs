using Application.Abstractions.Messaging;

namespace Application.Carts.GetCart;

public sealed record GetCartQuery(Guid UserId) : IQuery<CartResponse>;

public sealed record CartResponse(
    Guid Id,
    Guid OwnerId,
    ICollection<CartItemResponse> Items,
    decimal TotalAmount,
    string Currency);

public sealed record CartItemResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Currency,
    string MainPhotoPath);
