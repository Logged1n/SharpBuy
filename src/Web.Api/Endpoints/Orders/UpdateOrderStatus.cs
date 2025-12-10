using Application.Abstractions.Messaging;
using Application.Orders.UpdateOrderStatus;
using Domain.Orders;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

internal sealed class UpdateOrderStatus : IEndpoint
{
    public sealed record Request(OrderStatus NewStatus);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}/status", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateOrderStatusCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOrderStatusCommand(id, request.NewStatus);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization(policy => policy.RequireRole("Admin", "SalesManager"));
    }
}
