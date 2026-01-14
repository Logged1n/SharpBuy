using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Categories.GetAll;

public sealed record GetAllCategoriesQuery(int Page = 1, int PageSize = 10) : ICacheableQuery<PagedResult<CategoryListItem>>
{
    public string CacheKey => $"categories_page_{Page}_size_{PageSize}";

    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(30);
}

public sealed record CategoryListItem(Guid Id, string Name, int ProductCount);
