using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.RemoveRole;

internal sealed class RemoveRoleFromUserCommandHandler(
    UserManager<ApplicationUser> userManager)
    : ICommandHandler<RemoveRoleFromUserCommand>
{
    public async Task<Result> Handle(RemoveRoleFromUserCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        // Check if user has the role
        bool isInRole = await userManager.IsInRoleAsync(user, command.RoleName);
        if (!isInRole)
        {
            return Result.Failure(UserErrors.DoesNotHaveRole(command.RoleName));
        }

        // Prevent removing the last Admin role (ensure at least one admin exists)
        if (command.RoleName == Roles.Admin)
        {
            IList<ApplicationUser> admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
            if (admins.Count == 1 && admins[0].Id == user.Id)
            {
                return Result.Failure(Error.Problem(
                    "Users.CannotRemoveLastAdmin",
                    "Cannot remove the Admin role from the last administrator."));
            }
        }

        IdentityResult result = await userManager.RemoveFromRoleAsync(user, command.RoleName);

        if (!result.Succeeded)
        {
            IEnumerable<string> errors = result.Errors.Select(e => e.Description);
            return Result.Failure(UserErrors.IdentityFailed(errors));
        }

        return Result.Success();
    }
}
