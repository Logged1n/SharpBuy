using Application.Abstractions.Messaging;
using Application.Reviews.GetByProduct;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reviews;

internal sealed class GetByProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/{productId:guid}/reviews", async (
            Guid productId,
            IQueryHandler<GetReviewsByProductQuery, List<ReviewResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetReviewsByProductQuery(productId);

            Result<List<ReviewResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(
                reviews => Results.Ok(reviews),
                CustomResults.Problem);
        })
        .WithTags(Tags.Reviews)
        .AllowAnonymous();
    }
}
