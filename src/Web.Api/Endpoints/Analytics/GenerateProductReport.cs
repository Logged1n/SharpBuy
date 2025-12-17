using Application.Abstractions.Messaging;
using Application.Abstractions.Reporting;
using Application.Analytics;
using Application.Analytics.GetProductAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GenerateProductReport : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/reports/products", async (
            Request request,
            IQueryHandler<GetProductAnalyticsQuery, ProductAnalyticsResponse> handler,
            IPdfGenerator pdfGenerator,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<Granularity>(request.Granularity, true, out Granularity granularity))
            {
                return Results.BadRequest("Invalid granularity. Use: Day, Week, or Month");
            }

            var query = new GetProductAnalyticsQuery(
                request.StartDate,
                request.EndDate,
                granularity);

            Result<ProductAnalyticsResponse> result = await handler.Handle(query, cancellationToken);

            if (result.IsFailure)
            {
                return Results.Problem(detail: result.Error.Description, statusCode: 400);
            }

            byte[] pdfBytes = await pdfGenerator.GeneratePdfAsync(
                "Reports/ProductReport",
                result.Value,
                cancellationToken);

            string fileName = $"product-report-{DateTime.Now:yyyy-MM-dd}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
