using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Products.GetAll;

public sealed record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    List<Guid>? CategoryIds = null) : ICacheableQuery<PagedResult<ProductListItem>>
{
    public string CacheKey
    {
        get
        {
            string categoryIdsString = CategoryIds is not null && CategoryIds.Count > 0
                ? string.Join("_", CategoryIds.OrderBy(id => id))
                : "none";

#pragma warning disable CA1308 // Normalize strings to uppercase
            string searchTermString = string.IsNullOrWhiteSpace(SearchTerm)
                ? "none"
                : SearchTerm.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            return $"products_page_{Page}_size_{PageSize}_search_{searchTermString}_categories_{categoryIdsString}";
        }
    }

    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(10);
}

public sealed record ProductListItem(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    string MainPhotoPath);
