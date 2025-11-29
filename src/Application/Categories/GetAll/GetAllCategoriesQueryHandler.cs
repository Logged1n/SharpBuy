using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Categories.GetAll;

internal sealed class GetAllCategoriesQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAllCategoriesQuery, PagedResult<CategoryListItem>>
{
    public async Task<Result<PagedResult<CategoryListItem>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Categories.CountAsync(cancellationToken);

        List<CategoryListItem> categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CategoryListItem(c.Id, c.Name, c.Products.Count))
            .ToListAsync(cancellationToken);

        return PagedResult<CategoryListItem>.Create(categories, query.Page, query.PageSize, totalCount);
    }
}
