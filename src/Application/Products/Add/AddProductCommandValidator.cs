using FluentValidation;

namespace Application.Products.Add;

internal sealed class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).NotNull();
        RuleFor(x => x.Price.Amount).GreaterThan(0);
        RuleFor(x => x.Price.Currency).NotEmpty().Length(3);
        RuleFor(x => x.MainPhotoPath).NotEmpty();
    }
}
