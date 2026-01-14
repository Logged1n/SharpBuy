using Domain.Users;
using FluentValidation;

namespace Application.Users.AddRole;

internal sealed class AddRoleToUserCommandValidator : AbstractValidator<AddRoleToUserCommand>
{
    private static readonly string[] ValidRoles = [Roles.Admin, Roles.SalesManager, Roles.Salesman, Roles.Client];

    public AddRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}");
    }
}
