using Application.Abstractions.Messaging;

namespace Application.Users.AddRole;

public sealed record AddRoleToUserCommand(Guid UserId, string RoleName) : ICommand;
