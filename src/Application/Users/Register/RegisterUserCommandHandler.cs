using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Addresses;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.DomainUsers.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var domainUser = User.Create(
            command.Email,
            command.FirstName,
            command.LastName,
            command.PhoneNumber);

        if (command.PrimaryAddress is not null)
        {
            domainUser.AddAddress(
                command.PrimaryAddress.Line1,
                command.PrimaryAddress.Line2,
                command.PrimaryAddress.City,
                command.PrimaryAddress.PostalCode,
                command.PrimaryAddress.Country);
        }
        context.Set<ApplicationUser>().Add(new ApplicationUser
        {
            DomainUserId = domainUser.Id,
            DomainUser = domainUser,
            PasswordHash = passwordHasher.Hash(command.Password)
        });

        domainUser.Raise(new UserRegisteredDomainEvent(domainUser.Id));

        context.DomainUsers.Add(domainUser);

        await context.SaveChangesAsync(cancellationToken);

        return domainUser.Id;
    }
}
