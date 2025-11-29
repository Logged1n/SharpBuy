using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Categories.GetAll;

public sealed record GetAllCategoriesQuery(int Page = 1, int PageSize = 10) : IQuery<PagedResult<CategoryListItem>>;

public sealed record CategoryListItem(Guid Id, string Name, int ProductCount);
