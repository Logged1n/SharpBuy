using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Addresses;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.UpdateProfile;

internal sealed class UpdateUserProfileCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<UpdateUserProfileCommand>
{
    public async Task<Result> Handle(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        DomainUser? user = await dbContext.DomainUsers
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        ApplicationUser? applicationUser = await dbContext.ApplicationUsers
            .FirstOrDefaultAsync(au => au.DomainUserId == user.Id, cancellationToken);

        if (applicationUser is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        // Update user basic info
        Result updateResult = user.Update(command.FirstName, command.LastName);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Update email if changed
        if (applicationUser.Email != command.Email)
        {
            applicationUser.Email = command.Email;
            applicationUser.UserName = command.Email;
            applicationUser.NormalizedEmail = command.Email.ToUpperInvariant();
            applicationUser.NormalizedUserName = command.Email.ToUpperInvariant();
        }

        // Handle address
        if (command.Address is not null)
        {
            Address? existingAddress = await dbContext.Addresses
                .FirstOrDefaultAsync(a => a.UserId == user.Id, cancellationToken);

            if (existingAddress is not null)
            {
                // Update existing address
                Result addressUpdateResult = existingAddress.Update(
                    command.Address.Line1,
                    command.Address.Line2,
                    command.Address.City,
                    command.Address.PostalCode,
                    command.Address.Country);

                if (addressUpdateResult.IsFailure)
                {
                    return addressUpdateResult;
                }
            }
            else
            {
                // Create new address
                var newAddress = Address.Create(command.Address);
                newAddress.UserId = user.Id;
                dbContext.Addresses.Add(newAddress);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
