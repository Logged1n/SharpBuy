using Application.Abstractions.Messaging;

namespace Application.Carts.Clear;

public sealed record ClearCartCommand(Guid UserId) : ICommand;
