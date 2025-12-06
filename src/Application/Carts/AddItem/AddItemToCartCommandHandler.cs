using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Carts;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Carts.AddItem;

internal sealed class AddItemToCartCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<AddItemToCartCommand>
{
    public async Task<Result> Handle(AddItemToCartCommand command, CancellationToken cancellationToken)
    {
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

        // For anonymous users, we only validate - the cart is managed in frontend
        if (command.UserId is null)
        {
            return Result.Success();
        }

        // For authenticated users, manage cart in database
        // Note: Cart should already exist from user registration (DomainUser.Create automatically creates one)
        Cart? cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.OwnerId == command.UserId.Value, cancellationToken);

        if (cart is null)
        {
            // This should rarely happen, but handle it gracefully
            // Carts are created during user registration in DomainUser.Create()
            return Result.Failure(Error.NotFound("Cart.NotFound", "User's cart not found. Please contact support."));
        }

        Result addResult = cart.AddCartItem(command.ProductId, product.Price, command.Quantity);

        if (addResult.IsFailure)
        {
            return addResult;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
