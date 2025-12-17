using Application.Abstractions.Messaging;
using Application.Analytics;
using Application.Analytics.GetCustomerAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GetCustomerAnalytics : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/customers", async (
            Request request,
            IQueryHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<Granularity>(request.Granularity, true, out Granularity granularity))
            {
                return Results.BadRequest("Invalid granularity. Use: Day, Week, or Month");
            }

            var query = new GetCustomerAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                granularity);

            Result<CustomerAnalyticsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(
                analytics => Results.Ok(analytics),
                CustomResults.Problem);
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
