using Application.Abstractions.Messaging;

namespace Application.Users.RemoveRole;

public sealed record RemoveRoleFromUserCommand(Guid UserId, string RoleName) : ICommand;
