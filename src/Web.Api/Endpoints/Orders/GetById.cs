using Application.Abstractions.Messaging;
using Application.Orders.GetAll;
using Application.Orders.GetById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

public sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}", async (
            Guid id,
            IQueryHandler<GetOrderByIdQuery, OrderListItem> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrderByIdQuery(id);

            Result<OrderListItem> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization(policy => policy.RequireRole("Admin", "SalesManager"));
    }
}
