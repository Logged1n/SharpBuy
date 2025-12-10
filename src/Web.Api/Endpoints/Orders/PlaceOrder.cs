using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Orders.PlaceOrder;
using SharedKernel;
using SharedKernel.Dtos;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

internal sealed class PlaceOrder : IEndpoint
{
    public sealed record Request(
        Guid? ShippingAddressId,
        Guid? BillingAddressId,
        AddressDto? ShippingAddress,
        AddressDto? BillingAddress,
        string PaymentIntentId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
            Request request,
            ICommandHandler<PlaceOrderCommand, Guid> handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated == false)
                return Results.Unauthorized();

            string? userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out Guid parsedUserId))
            {
                return Results.Unauthorized();
            }

            var command = new PlaceOrderCommand(
                parsedUserId,
                request.ShippingAddressId,
                request.BillingAddressId,
                request.ShippingAddress,
                request.BillingAddress,
                request.PaymentIntentId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                orderId => Results.Created($"/orders/{orderId}", orderId),
                CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
