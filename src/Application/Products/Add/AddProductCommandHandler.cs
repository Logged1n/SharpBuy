using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Add;
internal sealed class AddProductCommandHandler(
    IApplicationDbContext dbContext,
    ICacheInvalidator cacheInvalidator) : ICommandHandler<AddProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        // Validate categories exist
        if (command.CategoryIds.Any())
        {
            List<Guid> existingCategoryIds = await dbContext.Categories
                .Where(c => command.CategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            if (existingCategoryIds.Count != command.CategoryIds.Count)
                return Result.Failure<Guid>(ProductErrors.InvalidCategories);
        }

        var product = Product.Create(command.Name, command.Description, command.Quantity,
            command.Price.Amount, command.Price.Currency, command.MainPhotoPath);

        // Add product to categories
        foreach (Guid categoryId in command.CategoryIds)
        {
            Result result = product.AddToCategory(categoryId);
            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);
        }

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate product list caches (all variations)
        await cacheInvalidator.InvalidateByPatternAsync("products_*", cancellationToken);

        // Invalidate affected category caches
        foreach (Guid categoryId in command.CategoryIds)
        {
            await cacheInvalidator.InvalidateAsync($"category_{categoryId}", cancellationToken);
        }
        await cacheInvalidator.InvalidateByPatternAsync("categories_*", cancellationToken);

        return product.Id;
    }
}
