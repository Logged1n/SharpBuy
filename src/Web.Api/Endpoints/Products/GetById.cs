using Application.Abstractions.Messaging;
using Application.Products.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{id:guid}", async (
            Guid id,
            IQueryHandler<GetProductByIdQuery, ProductResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProductByIdQuery(id);

            Result<ProductResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .AllowAnonymous();
    }
}
