using Application.Abstractions.Emails;
using Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.Users;

internal sealed class EmailVerificationLinkFactory(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator) : IEmailVerificationLinkFactory
{
    public string Create(EmailVerificationToken emailVerificationToken)
    {
        string? verificationLink = linkGenerator.GetUriByName(
            httpContextAccessor.HttpContext!,
            "VerifyEmail",
            new { token = emailVerificationToken.Id });

        return verificationLink ?? throw new Exception("Failed to generate verification link.");
    }
}
