using FluentValidation;

namespace Application.Orders.PlaceOrder;

internal sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PaymentIntentId).NotEmpty();
    }
}
