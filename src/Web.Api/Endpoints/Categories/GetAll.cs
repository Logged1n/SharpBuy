using Application.Abstractions.Messaging;
using Application.Categories.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Categories;

public sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (
            int page,
            int pageSize,
            IQueryHandler<GetAllCategoriesQuery, PagedResult<CategoryListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllCategoriesQuery(page, pageSize);

            Result<PagedResult<CategoryListItem>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .AllowAnonymous();
    }
}
