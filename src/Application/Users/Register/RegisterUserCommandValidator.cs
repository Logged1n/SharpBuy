using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        //RuleFor(c => c.PrimaryAddress).NotEmpty()
        //    .When(c => c.AdditionalAddresses is null || c.AdditionalAddresses.Count == 0);
        RuleFor(c => c.PhoneNumber)
            .Length(9)
            .When(c => !string.IsNullOrEmpty(c.PhoneNumber));
    }
}
