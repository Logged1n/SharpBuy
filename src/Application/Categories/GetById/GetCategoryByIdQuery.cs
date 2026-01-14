using Application.Abstractions.Messaging;

namespace Application.Categories.GetById;

public sealed record GetCategoryByIdQuery(Guid Id) : ICacheableQuery<CategoryResponse>
{
    public string CacheKey => $"category_{Id}";

    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(30);
}

public sealed record CategoryResponse(Guid Id, string Name, int ProductCount);
