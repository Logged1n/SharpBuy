using Application.Abstractions.Messaging;
using Application.Carts.RemoveItem;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Carts;

public sealed class RemoveItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("carts/items/{productId:guid}", async (
            Guid productId,
            ICommandHandler<RemoveItemFromCartCommand> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var command = new RemoveItemFromCartCommand(userId, productId);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Carts)
        .RequireAuthorization();
    }
}
