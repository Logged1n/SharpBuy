using Domain.Emails;

namespace Application.Abstractions.Emails;

public interface IEmailService
{
    Task<bool> SendEmailAsync(Email email);
}
