using Domain.Users;
using FluentValidation;

namespace Application.Users.RemoveRole;

internal sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    private static readonly string[] ValidRoles = [Roles.Admin, Roles.SalesManager, Roles.Salesman, Roles.Client];

    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}");
    }
}
