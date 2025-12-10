using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Payments;
using Domain.Carts;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.CreatePaymentIntent;

internal sealed class CreatePaymentIntentCommandHandler(
    IApplicationDbContext dbContext,
    IPaymentService paymentService) : ICommandHandler<CreatePaymentIntentCommand, string>
{
    public async Task<Result<string>> Handle(CreatePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        // Get user
        ApplicationUser? user = await dbContext.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFound(command.UserId));
        }

        // Get cart with items
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            return Result.Failure<string>(CartErrors.EmptyCart);
        }

        // Calculate total
        decimal totalAmount = cart.Total.Amount;
        string currency = cart.Total.Currency;

        // Create payment intent
        Result<string> result = await paymentService.CreatePaymentIntentAsync(
            totalAmount,
            currency,
            user.Email!,
            cancellationToken);

        return result;
    }
}
