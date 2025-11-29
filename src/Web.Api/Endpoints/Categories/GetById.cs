using Application.Abstractions.Messaging;
using Application.Categories.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Categories;

public sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories/{id:guid}", async (
            Guid id,
            IQueryHandler<GetCategoryByIdQuery, CategoryResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoryByIdQuery(id);

            Result<CategoryResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .AllowAnonymous();
    }
}
