using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher<ApplicationUser> passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await context.Set<ApplicationUser>()
            .Where(u => u.NormalizedEmail == command.Email)
            .Include(u => u.DomainUser)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        if (!user.EmailConfirmed)
            return Result.Failure<string>(UserErrors.EmailNotVerified);


        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, command.Password) != PasswordVerificationResult.Success)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        string token = await tokenProvider.Create(user);

        return token;
    }
}
