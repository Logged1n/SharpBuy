using Application.Abstractions.Messaging;

namespace Application.Carts.UpdateQuantity;

public sealed record UpdateCartItemQuantityCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity) : ICommand;
