using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.VerifyEmail;
internal sealed class VerifyEmailCommandHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        EmailVerificationToken? token = await dbContext.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(t => t.Id == request.TokenId, cancellationToken);

        if (token is null || token.ExpiresOnUtc < dateTimeProvider.UtcNow || token.User.EmailVerified)
            return Result.Failure(EmailErrors.InvalidToken);

        token.User.VerifyEmail();
        dbContext.EmailVerificationTokens.Remove(token);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
