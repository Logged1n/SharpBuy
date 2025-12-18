using Application.Abstractions.Reporting;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web.Api.Endpoints.Analytics;

internal sealed class GetPdfJobStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("analytics/pdf-jobs/{jobId}", async (
            string jobId,
            ICachedPdfGenerator pdfGenerator,
            CancellationToken cancellationToken) =>
        {
            PdfJobStatus? status = await pdfGenerator.GetJobStatusAsync(jobId, cancellationToken);

            if (status is null)
            {
                return Results.NotFound(new { message = "PDF job not found" });
            }

            return Results.Ok(new
            {
                jobId = status.JobId,
                state = status.State.ToString(),
                pdfCacheKey = status.PdfCacheKey,
                errorMessage = status.ErrorMessage,
                createdAt = status.CreatedAt,
                completedAt = status.CompletedAt
            });
        })
        .WithTags(Tags.Analytics)
        .WithName("GetPdfJobStatus")
        .RequireAuthorization();
    }
}
