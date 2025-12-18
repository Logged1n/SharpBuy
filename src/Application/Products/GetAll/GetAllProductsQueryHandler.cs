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
        IQueryable<Domain.Products.Product> productsQuery = dbContext.Products.AsNoTracking();

        // Filter by search term (name or description)
        // Using Contains which is case-sensitive by default
        // For case-insensitive search in production, consider:
        // 1. Adding full-text search index (pg_trgm extension)
        // 2. Using case-insensitive collation on columns
        // 3. Adding Npgsql.EntityFrameworkCore.PostgreSQL to Application layer for ILike support
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                p.Description.Contains(query.SearchTerm));
        }

        // Filter by categories
        if (query.CategoryIds is not null && query.CategoryIds.Count > 0)
        {
            productsQuery = productsQuery.Where(p =>
                p.Categories.Any(pc => query.CategoryIds.Contains(pc.CategoryId)));
        }

        int totalCount = await productsQuery.CountAsync(cancellationToken);

        List<ProductListItem> products = await productsQuery
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
                p.MainPhotoPath,
                p.PhotoPaths))
            .ToListAsync(cancellationToken);

        return PagedResult<ProductListItem>.Create(products, query.Page, query.PageSize, totalCount);
    }
}
