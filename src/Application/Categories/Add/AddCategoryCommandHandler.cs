using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.Add;

internal sealed class AddCategoryCommandHandler(
    IApplicationDbContext dbContext,
    ICacheInvalidator cacheInvalidator)
    : ICommandHandler<AddCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        bool exists = await dbContext.Categories
            .AnyAsync(c => c.Name == command.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(CategoryErrors.NameAlreadyExists);
        }

        var category = Category.Create(command.Name);

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Invalidate category list caches (all variations)
        await cacheInvalidator.InvalidateByPatternAsync("categories_*", cancellationToken);

        return category.Id;
    }
}
