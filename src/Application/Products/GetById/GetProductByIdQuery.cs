using Application.Abstractions.Messaging;

namespace Application.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;

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
