using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Delete;

internal sealed class DeleteProductCommandHandler(
    IApplicationDbContext dbContext,
    ICacheInvalidator cacheInvalidator)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await dbContext.Products
            .Include(p => p.Inventory)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.ProductNotFound(command.Id));
        }

        // Get category IDs before deletion for cache invalidation
        var categoryIds = product.Categories.Select(pc => pc.CategoryId).ToList();

        // Soft delete - mark inventory as deleted
        if (product.Inventory is not null)
        {
            dbContext.Inventories.Remove(product.Inventory);
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate specific product cache
        await cacheInvalidator.InvalidateAsync($"product_{command.Id}", cancellationToken);

        // Invalidate product list caches (all variations)
        await cacheInvalidator.InvalidateByPatternAsync("products_*", cancellationToken);

        // Invalidate affected category caches
        foreach (Guid categoryId in categoryIds)
        {
            await cacheInvalidator.InvalidateAsync($"category_{categoryId}", cancellationToken);
        }
        await cacheInvalidator.InvalidateByPatternAsync("categories_*", cancellationToken);

        return Result.Success();
    }
}
