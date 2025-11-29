using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.GetById;

internal sealed class GetCategoryByIdQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        CategoryResponse? category = await dbContext.Categories
            .Where(c => c.Id == query.Id)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Products.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryResponse>(CategoryErrors.CategoryNotFound(query.Id));
        }

        return category;
    }
}
