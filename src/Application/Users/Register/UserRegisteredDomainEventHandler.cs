using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Domain.Carts;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailVerificationLinkFactory _emailVerificationLinkFactory;

    public UserRegisteredDomainEventHandler(
        ILogger<UserRegisteredDomainEventHandler> logger,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService,
        IApplicationDbContext dbContext,
        IEmailVerificationLinkFactory emailVerificationLinkFactory)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _emailService = emailService;
        _dbContext = dbContext;
        _emailVerificationLinkFactory = emailVerificationLinkFactory;
    }
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == notification.UserId, cancellationToken);

        if(user is null)
        {
            _logger.LogError("Failed to handle UserRegisteredDomainEvent for User ID {UserId}.", notification.UserId);
            return;
        }

        _dbContext.Carts.Add(new Cart()
        {
            OwnerId = user.Id,
            Owner = user,
        });

        DateTime utcNow = _dateTimeProvider.UtcNow;
        var verificationToken = new EmailVerificationToken()
        {
            UserId = user.Id,
            Id = Guid.NewGuid(),
            CreatedOnUtc = utcNow,
            ExpiresOnUtc = utcNow.AddDays(1)
        };
        _dbContext.EmailVerificationTokens.Add(verificationToken);

        string verificationLink = _emailVerificationLinkFactory.Create(verificationToken);
        string body = $"Hello {user.FirstName},<br/>Thank you for registering with us!<br/>We are excited to have you on board. To verify your account <a href='{verificationLink}'>click here</a>.";

        Result result = await _emailService.SendEmailAsync(
            user.Email,
            "Welcome to our service!",
            body);

        if(result.IsSuccess)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Welcome email sent to {Email} for User ID {UserId}.", user.Email, user.Id);
        }
        else
        {
            _logger.LogError("Failed to send welcome email to {Email} for User ID {UserId}.", user.Email, user.Id);
        }
            
    }
}
