using Application.Abstractions.Messaging;
using Application.Carts.AddItem;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Carts;

public sealed class AddItem : IEndpoint
{
    public sealed record Request(Guid ProductId, int Quantity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("carts/items", async (
            Request request,
            ICommandHandler<AddItemToCartCommand> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            Guid? userId = null;
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdClaim, out Guid parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var command = new AddItemToCartCommand(
                userId,
                request.ProductId,
                request.Quantity);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.Ok(new { message = "Item added to cart" }),
                CustomResults.Problem);
        })
        .WithTags(Tags.Carts);
    }
}
