using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.DomainUsers.AnyAsync(u => u.Email == command.Email, cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);

        var domainUser = DomainUser.Create(
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

        context.DomainUsers.Add(domainUser);
        context.Carts.Add(domainUser.Cart);
        await context.SaveChangesAsync(cancellationToken);
        domainUser.Raise(new UserRegisteredDomainEvent(domainUser.Email));

        var appUser = new ApplicationUser
        {
            Id = domainUser.Id,
            UserName = command.Email,
            Email = command.Email,
            NormalizedEmail = command.Email.ToUpperInvariant(),
            PhoneNumber = command.PhoneNumber,
            DomainUserId = domainUser.Id,
            DomainUser = domainUser,
            EmailConfirmed = command.EmailConfirmed
        };

        IdentityResult createResult = command.EmailConfirmed && string.IsNullOrEmpty(command.Password)
            ? await userManager.CreateAsync(appUser)
            : await userManager.CreateAsync(appUser, command.Password);
        ApplicationUser? registeredUser = await userManager.FindByEmailAsync(command.Email);
        IdentityResult addRoleResult = await userManager.AddToRoleAsync(registeredUser!, Roles.Client);

        if (!createResult.Succeeded || !addRoleResult.Succeeded)
        {
            IEnumerable<string> errors = createResult.Errors.Concat(addRoleResult.Errors)
                .Select(e => e.Description);
            return Result.Failure<Guid>(UserErrors.IdentityFailed(errors));
        }

        return domainUser.Id;
    }
}
