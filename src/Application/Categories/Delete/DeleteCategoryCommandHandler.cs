using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.Delete;

internal sealed class DeleteCategoryCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        Category? category = await dbContext.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.CategoryNotFound(command.Id));
        }

        if (category.Products.Any())
        {
            return Result.Failure(CategoryErrors.CannotDelete);
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
