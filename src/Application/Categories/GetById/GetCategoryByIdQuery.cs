using Application.Abstractions.Messaging;

namespace Application.Categories.GetById;

public sealed record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryResponse>;

public sealed record CategoryResponse(Guid Id, string Name, int ProductCount);
