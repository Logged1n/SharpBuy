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
            IQueryHandler<GetAllProductsQuery, PagedResult<ProductListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllProductsQuery(page, pageSize);

            Result<PagedResult<ProductListItem>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .AllowAnonymous();
    }
}
