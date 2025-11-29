using Application.Abstractions.Messaging;

namespace Application.Carts.AddItem;

public sealed record AddItemToCartCommand(
    Guid? UserId,
    Guid ProductId,
    int Quantity) : ICommand;
