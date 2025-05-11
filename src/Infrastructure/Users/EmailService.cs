using System.Net;
using System.Net.Mail;
using Application.Abstractions.Emails;
using Domain.Users;
using FluentEmail.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Infrastructure.Users;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public EmailService(IFluentEmail fluentEmail)
    {
        _fluentEmail = fluentEmail;
    }

    public async Task<Result> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            await _fluentEmail.To(to)
                .Subject(subject)
                .Body(body, isHtml: true)
                .SendAsync();

            return Result.Success();
        }
        catch (SmtpException ex)
        {
            return Result.Failure(EmailErrors.SendingFailed(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure(
                Error.Failure(
                    "EmailService.SendEmailAsync",
                    $"An unexpected error occured: {ex.Message}"));
        }
    }
}
