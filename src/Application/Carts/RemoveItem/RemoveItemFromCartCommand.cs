using Application.Abstractions.Messaging;

namespace Application.Carts.RemoveItem;

public sealed record RemoveItemFromCartCommand(Guid UserId, Guid ProductId) : ICommand;
