using System.Net;
using System.Net.Mail;
using Application.Abstractions.Emails;
using Domain.Emails;
using Microsoft.Extensions.Options;

namespace Infrastructure.Emails;

public class EmailService : IEmailService
{
    private readonly IOptions<EmailOptions> _emailConfiguration;

    public EmailService(IOptions<EmailOptions> emailConfiguration)
    {
        _emailConfiguration = emailConfiguration;
    }
    public async Task<bool> SendEmailAsync(Email email)
    {
        using var client = new SmtpClient()
        {
            Host = _emailConfiguration.Value.SmtpServer,
            Port = _emailConfiguration.Value.SmtpPort,
            EnableSsl = true,
            Credentials = new NetworkCredential(_emailConfiguration.Value.SmtpUsername, _emailConfiguration.Value.SmtpPassword)
        };

        using var message = new MailMessage()
        {
            From = new MailAddress(_emailConfiguration.Value.FromAddress, _emailConfiguration.Value.FromName),
            Subject = email.Subject,
            Body = email.Body,
            IsBodyHtml = true
        };
        message.To.Add(email.To);
        try
        {
            await client.SendMailAsync(message);
            return true;
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return false;
        }
    }
}
