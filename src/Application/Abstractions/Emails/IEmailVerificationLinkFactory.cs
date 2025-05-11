using Domain.Users;

namespace Application.Abstractions.Emails;

public interface IEmailVerificationLinkFactory
{
    string Create(EmailVerificationToken emailVerificationToken);
}
