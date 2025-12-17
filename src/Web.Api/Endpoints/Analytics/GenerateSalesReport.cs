using Application.Abstractions.Messaging;
using Application.Abstractions.Reporting;
using Application.Analytics;
using Application.Analytics.GetSalesAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GenerateSalesReport : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/reports/sales", async (
            Request request,
            IQueryHandler<GetSalesAnalyticsQuery, SalesAnalyticsResponse> handler,
            IPdfGenerator pdfGenerator,
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

            if (result.IsFailure)
            {
                return Results.Problem(detail: result.Error.Description, statusCode: 400);
            }

            byte[] pdfBytes = await pdfGenerator.GeneratePdfAsync(
                "Reports/SalesReport",
                result.Value,
                cancellationToken);

            string fileName = $"sales-report-{DateTime.Now:yyyy-MM-dd}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
