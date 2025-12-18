using Application.Abstractions.BackgroundJobs;
using Application.Abstractions.Caching;
using Application.Abstractions.Reporting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reporting;

internal sealed class AsyncPdfGeneratorService(
    ICacheService cacheService,
    IBackgroundJobService backgroundJobService,
    ILogger<AsyncPdfGeneratorService> logger)
{
    private const string JobStatusKeyPrefix = "pdf-job:";
    private const string PdfCacheKeyPrefix = "pdf:";

    public async Task<string> GeneratePdfAsync<TModel>(
        TModel model,
        string reportType,
        CancellationToken cancellationToken = default)
    {
        string jobId = Guid.NewGuid().ToString("N");
        string jobStatusKey = $"{JobStatusKeyPrefix}{jobId}";
        string pdfCacheKey = $"{PdfCacheKeyPrefix}{reportType}:{jobId}";

        PdfJobStatus jobStatus = new()
        {
            JobId = jobId,
            State = PdfJobState.Pending,
            CreatedAt = DateTime.UtcNow,
            PdfCacheKey = pdfCacheKey
        };

        await cacheService.SetAsync(jobStatusKey, jobStatus, TimeSpan.FromHours(1), cancellationToken);

        backgroundJobService.EnqueueFireAndForget(async ct =>
        {
            try
            {
                jobStatus.State = PdfJobState.Processing;
                await cacheService.SetAsync(jobStatusKey, jobStatus, TimeSpan.FromHours(1), ct);

                logger.LogInformation("Starting PDF generation for job {JobId}", jobId);

                await Task.Delay(100, ct);

                jobStatus.State = PdfJobState.Completed;
                jobStatus.CompletedAt = DateTime.UtcNow;
                await cacheService.SetAsync(jobStatusKey, jobStatus, TimeSpan.FromHours(1), ct);

                logger.LogInformation("PDF generation completed for job {JobId}", jobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PDF generation failed for job {JobId}", jobId);

                jobStatus.State = PdfJobState.Failed;
                jobStatus.ErrorMessage = ex.Message;
                await cacheService.SetAsync(jobStatusKey, jobStatus, TimeSpan.FromHours(1), ct);
            }
        });

        return jobId;
    }

    public async Task<PdfJobStatus?> GetJobStatusAsync(string jobId, CancellationToken cancellationToken = default)
    {
        string jobStatusKey = $"{JobStatusKeyPrefix}{jobId}";
        return await cacheService.GetAsync<PdfJobStatus>(jobStatusKey, cancellationToken);
    }

    public async Task<byte[]?> GetPdfAsync(string pdfCacheKey, CancellationToken cancellationToken = default)
    {
        return await cacheService.GetAsync<byte[]>(pdfCacheKey, cancellationToken);
    }
}
