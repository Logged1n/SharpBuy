using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.VerifyEmail;
internal sealed class VerifyEmailCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        EmailVerificationToken? token = await dbContext.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(t => t.Id == request.TokenId, cancellationToken);

        if (token is null || token.ExpiresOnUtc < DateTime.UtcNow || token.User.IsEmailVerified)
            return Result.Failure(EmailErrors.InvalidToken);

        token.User.IsEmailVerified = true;
        dbContext.EmailVerificationTokens.Remove(token);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
