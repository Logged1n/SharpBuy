using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Domain.Emails;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Users.Register;

internal sealed class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _dbContext;

    public UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger, IEmailService emailService, IApplicationDbContext dbContext)
    {
        _logger = logger;
        _emailService = emailService;
        _dbContext = dbContext;
    }
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == notification.UserId, cancellationToken);

        if(user is null)
        {
            _logger.LogError("Failed to handle UserRegisteredDomainEvent for User ID {UserId}.", notification.UserId);
            throw new Exception($"User with ID {notification.UserId} not found.");
        }
        var message = new Email()
        {
            To = user.Email,
            Subject = "Welcome to our service!",
            Body = $"Hello {user.FirstName},<br/>Thank you for registering with us!<br/>We are excited to have you on board."
        };
        if(await _emailService.SendEmailAsync(message))
        {
            _logger.LogInformation("Welcome email sent to {Email} for User ID {UserId}.", user.Email, user.Id);
        }
        else
        {
            _logger.LogError("Failed to send welcome email to {Email} for User ID {UserId}.", user.Email, user.Id);
        }
    }
}
