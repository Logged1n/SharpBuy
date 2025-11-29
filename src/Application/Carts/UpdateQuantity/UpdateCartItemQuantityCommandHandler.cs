using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.UpdateQuantity;

internal sealed class UpdateCartItemQuantityCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> Handle(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
    {
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(CartErrors.CartNotFound(command.UserId));
        }

        // Get product and check availability
        Product? product = await dbContext.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.ProductNotFound(command.ProductId));
        }

        // Check inventory availability
        if (product.Inventory.AvailableQuantity < command.Quantity)
        {
            return Result.Failure(CartErrors.InsufficientStock);
        }

        Result updateResult = cart.ChnageItemQuantity(command.ProductId, command.Quantity);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
