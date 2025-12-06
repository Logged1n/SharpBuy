using System.Security.Claims;
using Application.Abstractions.Messaging;

namespace Application.Users.Login.Google;

public sealed record LoginGoogleCommand(ClaimsPrincipal Principal) : ICommand<string>;
