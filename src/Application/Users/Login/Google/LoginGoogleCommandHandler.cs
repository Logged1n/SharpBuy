using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Register;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login.Google;

internal sealed class LoginGoogleCommandHandler(
    IApplicationDbContext dbContext,
    ITokenProvider tokenProvider,
    ICommandHandler<RegisterUserCommand, Guid> registerUserCommandHandler) : ICommandHandler<LoginGoogleCommand, string>
{
    public async Task<Result<string>> Handle(LoginGoogleCommand command, CancellationToken cancellationToken)
    {
        if (command.Principal is null)
            return Result.Failure<string>(UserErrors.IdentityFailed(["External Provider failed to authenticate."]));

        string? email = command.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<string>(UserErrors.IdentityFailed(["External Provider did not provide an email address."]));

        ApplicationUser? user = await dbContext.ApplicationUsers
            .Where(u => u.NormalizedEmail == email.ToUpperInvariant())
            .Include(u => u.DomainUser)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is not null)
            return await tokenProvider.Create(user);

        Result<Guid> registerResult = await registerUserCommandHandler.Handle(new RegisterUserCommand(
            email,
            command.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            command.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
            string.Empty,
            command.Principal.FindFirstValue(ClaimTypes.MobilePhone) ?? string.Empty,
            EmailConfirmed: true), cancellationToken);

        if (registerResult.IsFailure)
            return Result.Failure<string>(registerResult.Error);

        ApplicationUser? registeredUser = await dbContext.ApplicationUsers
            .Where(u => u.Id == registerResult.Value)
            .Include(u => u.DomainUser)
            .FirstOrDefaultAsync(u => u.DomainUserId == registerResult.Value, cancellationToken);

        if (registeredUser is null)
            return Result.Failure<string>(UserErrors.NotFound(registerResult.Value));

        return await tokenProvider.Create(registeredUser);


    }
}
