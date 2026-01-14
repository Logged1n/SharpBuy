using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.Update;

internal sealed class UpdateCategoryCommandHandler(
    IApplicationDbContext dbContext,
    ICacheInvalidator cacheInvalidator)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        Category? category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.CategoryNotFound(command.Id));
        }

        bool nameExists = await dbContext.Categories
            .AnyAsync(c => c.Name == command.Name && c.Id != command.Id, cancellationToken);

        if (nameExists)
        {
            return Result.Failure(CategoryErrors.NameAlreadyExists);
        }

        category.Update(command.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate specific category cache
        await cacheInvalidator.InvalidateAsync($"category_{command.Id}", cancellationToken);

        // Invalidate category list caches (all variations)
        await cacheInvalidator.InvalidateByPatternAsync("categories_*", cancellationToken);

        // Invalidate product list caches (since they may include category names)
        await cacheInvalidator.InvalidateByPatternAsync("products_*", cancellationToken);

        return Result.Success();
    }
}
