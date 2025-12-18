using Application.Abstractions.Messaging;
using Application.Abstractions.Reporting;
using Application.Analytics;
using Application.Analytics.GetOrderAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GenerateOrderReport : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/reports/orders", async (
            Request request,
            IQueryHandler<GetOrderAnalyticsQuery, OrderAnalyticsResponse> handler,
            ICachedPdfGenerator pdfGenerator,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<Granularity>(request.Granularity, true, out Granularity granularity))
            {
                return Results.BadRequest("Invalid granularity. Use: Day, Week, or Month");
            }

            var query = new GetOrderAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                granularity);

            Result<OrderAnalyticsResponse> result = await handler.Handle(query, cancellationToken);

            if (result.IsFailure)
            {
                return Results.Problem(detail: result.Error.Description, statusCode: 400);
            }

            string cacheKey = $"orders:{request.StartDate:yyyy-MM-dd}:{request.EndDate:yyyy-MM-dd}:{request.Granularity}";

            string jobId = await pdfGenerator.GeneratePdfAsync(
                "Reports/OrderReport",
                result.Value,
                cacheKey,
                cancellationToken);

            return Results.Accepted($"/analytics/pdf-jobs/{jobId}", new
            {
                jobId,
                message = "PDF generation started. Check the status endpoint for progress.",
                statusUrl = $"/analytics/pdf-jobs/{jobId}"
            });
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
