using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        //RuleFor(c => c.PrimaryAddress).NotEmpty()
        //    .When(c => c.AdditionalAddresses is null || c.AdditionalAddresses.Count == 0);
        RuleFor(c => c.PhoneNumber)
            .Length(9)
            .When(c => !string.IsNullOrEmpty(c.PhoneNumber));
    }
}
