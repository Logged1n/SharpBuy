using FluentValidation;

namespace Application.Users.UpdateProfile;

internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.Line1).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Address!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Address!.PostalCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Address!.Country).NotEmpty().MaximumLength(100);
        });
    }
}
