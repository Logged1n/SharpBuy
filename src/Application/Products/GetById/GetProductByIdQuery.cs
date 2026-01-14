using Application.Abstractions.Messaging;

namespace Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : ICacheableQuery<ProductResponse>
{
    public string CacheKey => $"product_{Id}";

    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(15);
}

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    int StockQuantity,
    string MainPhotoPath,
    ICollection<CategoryInfo> Categories);

public sealed record CategoryInfo(Guid Id, string Name);
