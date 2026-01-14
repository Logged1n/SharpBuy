using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Application.Users.AddRole;

internal sealed class AddRoleToUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager)
    : ICommandHandler<AddRoleToUserCommand>
{
    public async Task<Result> Handle(AddRoleToUserCommand command, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        // Check if role exists
        bool roleExists = await roleManager.RoleExistsAsync(command.RoleName);
        if (!roleExists)
        {
            return Result.Failure(UserErrors.RoleNotFound(command.RoleName));
        }

        // Check if user already has the role
        bool isInRole = await userManager.IsInRoleAsync(user, command.RoleName);
        if (isInRole)
        {
            return Result.Failure(UserErrors.AlreadyHasRole(command.RoleName));
        }

        IdentityResult result = await userManager.AddToRoleAsync(user, command.RoleName);

        if (!result.Succeeded)
        {
            IEnumerable<string> errors = result.Errors.Select(e => e.Description);
            return Result.Failure(UserErrors.IdentityFailed(errors));
        }

        return Result.Success();
    }
}
