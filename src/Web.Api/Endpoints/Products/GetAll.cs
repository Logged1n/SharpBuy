using Application.Abstractions.Messaging;
using Application.Products.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products", async (
            int page,
            int pageSize,
            string? searchTerm,
            [Microsoft.AspNetCore.Mvc.FromQuery] Guid[]? categoryIds,
            IQueryHandler<GetAllProductsQuery, PagedResult<ProductListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            List<Guid>? categoryIdList = categoryIds?.ToList();
            var query = new GetAllProductsQuery(page, pageSize, searchTerm, categoryIdList);

            Result<PagedResult<ProductListItem>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .AllowAnonymous();
    }
}
