using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.RemoveItem;

internal sealed class RemoveItemFromCartCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<RemoveItemFromCartCommand>
{
    public async Task<Result> Handle(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(CartErrors.CartNotFound(command.UserId));
        }

        Result removeResult = cart.RemoveCartItem(command.ProductId);

        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
