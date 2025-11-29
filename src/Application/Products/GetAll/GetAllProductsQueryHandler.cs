using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetAll;

internal sealed class GetAllProductsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAllProductsQuery, PagedResult<ProductListItem>>
{
    public async Task<Result<PagedResult<ProductListItem>>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        int totalCount = await dbContext.Products.CountAsync(cancellationToken);

        List<ProductListItem> products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductListItem(
                p.Id,
                p.Name,
                p.Description,
                p.Price.Amount,
                p.Price.Currency,
                p.Inventory.Quantity,
                p.MainPhotoPath))
            .ToListAsync(cancellationToken);

        return PagedResult<ProductListItem>.Create(products, query.Page, query.PageSize, totalCount);
    }
}
