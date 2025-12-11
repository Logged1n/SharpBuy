using Application.Abstractions.Messaging;
using Application.Reviews.Add;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Reviews;

internal sealed class Add : IEndpoint
{
    public sealed record Request(
        Guid ProductId,
        int Score,
        string Title,
        string? Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews", async (
            Request request,
            ICommandHandler<AddReviewCommand, Guid> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var command = new AddReviewCommand(
                userId,
                request.ProductId,
                request.Score,
                request.Title,
                request.Description);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                reviewId => Results.Created($"/reviews/{reviewId}", reviewId),
                CustomResults.Problem);
        })
        .WithTags(Tags.Reviews)
        .RequireAuthorization();
    }
}
