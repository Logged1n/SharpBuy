using Application.Abstractions.Messaging;
using Application.Analytics;
using Application.Analytics.GetSalesAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GetSalesAnalytics : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/sales", async (
            Request request,
            IQueryHandler<GetSalesAnalyticsQuery, SalesAnalyticsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<Granularity>(request.Granularity, true, out Granularity granularity))
            {
                return Results.BadRequest("Invalid granularity. Use: Day, Week, or Month");
            }

            var query = new GetSalesAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                granularity);

            Result<SalesAnalyticsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(
                analytics => Results.Ok(analytics),
                CustomResults.Problem);
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
