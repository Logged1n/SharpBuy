using Application.Abstractions.Messaging;
using Application.Carts.GetCart;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Carts;

public sealed class GetCart : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("carts", async (
            IQueryHandler<GetCartQuery, CartResponse> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var query = new GetCartQuery(userId);

            Result<CartResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Carts)
        .RequireAuthorization();
    }
}
