using Application.Abstractions.Messaging;
using Application.Abstractions.Reporting;
using Application.Analytics;
using Application.Analytics.GetCustomerAnalytics;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GenerateCustomerReport : IEndpoint
{
    public sealed record Request(
        DateTime StartDate,
        DateTime EndDate,
        string Granularity);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("analytics/reports/customers", async (
            Request request,
            IQueryHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsResponse> handler,
            IPdfGenerator pdfGenerator,
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

            if (result.IsFailure)
            {
                return Results.Problem(detail: result.Error.Description, statusCode: 400);
            }

            byte[] pdfBytes = await pdfGenerator.GeneratePdfAsync(
                "Reports/CustomerReport",
                result.Value,
                cancellationToken);

            string fileName = $"customer-report-{DateTime.Now:yyyy-MM-dd}.pdf";
            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithTags(Tags.Analytics)
        .RequireAuthorization();
    }
}
