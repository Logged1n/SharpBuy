using Application.Abstractions.Messaging;
using Application.Orders.GetAll;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Orders;

public sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            int page,
            int pageSize,
            IQueryHandler<GetAllOrdersQuery, PagedResult<OrderListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllOrdersQuery(page, pageSize);

            Result<PagedResult<OrderListItem>> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Orders)
        .RequireAuthorization(policy => policy.RequireRole("Admin", "SalesManager"));
    }
}
