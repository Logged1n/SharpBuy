using SharedKernel;

namespace Application.Abstractions.Emails;

public interface IEmailService
{
    Task<Result> SendEmailAsync(string to, string subject, string body);
}
