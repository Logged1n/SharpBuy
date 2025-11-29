using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await dbContext.Products
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.ProductNotFound(command.Id));
        }

        // Soft delete - mark inventory as deleted
        if (product.Inventory is not null)
        {
            dbContext.Inventories.Remove(product.Inventory);
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
