using Application.Abstractions.Reporting;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web.Api.Endpoints.Analytics;

internal sealed class DownloadPdf : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("analytics/pdf/{cacheKey}", async (
            string cacheKey,
            ICachedPdfGenerator pdfGenerator,
            CancellationToken cancellationToken) =>
        {
            byte[]? pdfBytes = await pdfGenerator.GetPdfAsync(cacheKey, cancellationToken);

            if (pdfBytes is null)
            {
                return Results.NotFound(new { message = "PDF not found or expired" });
            }

            string fileName = $"analytics-report-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

            return Results.File(pdfBytes, "application/pdf", fileName);
        })
        .WithTags(Tags.Analytics)
        .WithName("DownloadPdf")
        .RequireAuthorization();
    }
}
