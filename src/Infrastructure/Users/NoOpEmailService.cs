using Application.Abstractions.Emails;
using SharedKernel;

namespace Infrastructure.Users;

/// <summary>
/// A no-operation email service used during design-time (EF migrations) when SMTP is not configured.
/// </summary>
internal sealed class NoOpEmailService : IEmailService
{
    public Task<Result> SendEmailAsync(string to, string subject, string body)
    {
        return Task.FromResult(Result.Success());
    }
}
