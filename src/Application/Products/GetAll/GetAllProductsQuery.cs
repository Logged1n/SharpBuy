using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Products.GetAll;

public sealed record GetAllProductsQuery(int Page = 1, int PageSize = 10) : IQuery<PagedResult<ProductListItem>>;

public sealed record ProductListItem(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    string MainPhotoPath);
