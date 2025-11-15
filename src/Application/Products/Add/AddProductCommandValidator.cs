using FluentValidation;

namespace Application.Products.Add;
internal sealed class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Price).NotNull().Must(p => p.Amount > 0).WithMessage("Price must be greater than zero.");
        RuleFor(c => c.CategoryIds).NotEmpty().WithMessage("At least one category must be specified.");
    }
}
