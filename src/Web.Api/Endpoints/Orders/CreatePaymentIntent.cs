using System.Security.Claims;
using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Orders.CreatePaymentIntent;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

internal sealed class CreatePaymentIntent : IEndpoint
{
    public sealed record Response(string ClientSecret);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/payment-intent", async (
            ICommandHandler<CreatePaymentIntentCommand, string> handler,
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

            var command = new CreatePaymentIntentCommand(parsedUserId);

            Result<string> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                clientSecret => Results.Ok(new Response(clientSecret)),
                CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization();
    }
}
