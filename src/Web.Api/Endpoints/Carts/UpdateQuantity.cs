using Application.Abstractions.Messaging;
using Application.Carts.UpdateQuantity;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Carts;

public sealed class UpdateQuantity : IEndpoint
{
    public sealed record Request(int Quantity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("carts/items/{productId:guid}", async (
            Guid productId,
            Request request,
            ICommandHandler<UpdateCartItemQuantityCommand> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateCartItemQuantityCommand(
                userId,
                productId,
                request.Quantity);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Carts)
        .RequireAuthorization();
    }
}
