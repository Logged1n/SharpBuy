using System.Text.Json;
using Application.Abstractions.BackgroundJobs;
using Application.Abstractions.Caching;
using Application.Abstractions.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Reporting;

internal sealed class CachedPdfGeneratorService(
    ICacheService cacheService,
    IBackgroundJobService backgroundJobService,
    IServiceProvider serviceProvider,
    ILogger<CachedPdfGeneratorService> logger) : ICachedPdfGenerator
{
    private const string JobStatusKeyPrefix = "pdf-job:";
    private const string PdfCacheKeyPrefix = "pdf:";

    public async Task<string> GeneratePdfAsync<TModel>(
        string templateName,
        TModel model,
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        string pdfCacheKey = $"{PdfCacheKeyPrefix}{cacheKey}";

        byte[]? cachedPdf = await cacheService.GetAsync<byte[]>(pdfCacheKey, cancellationToken);
        if (cachedPdf is not null)
        {
            string existingJobId = Guid.NewGuid().ToString("N");
            string existingJobStatusKey = $"{JobStatusKeyPrefix}{existingJobId}";

            PdfJobStatus existingStatus = new()
            {
                JobId = existingJobId,
                State = PdfJobState.Completed,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                PdfCacheKey = pdfCacheKey
            };

            await cacheService.SetAsync(existingJobStatusKey, existingStatus, TimeSpan.FromHours(1), cancellationToken);
            return existingJobId;
        }

        string jobId = Guid.NewGuid().ToString("N");
        string jobStatusKey = $"{JobStatusKeyPrefix}{jobId}";

        PdfJobStatus jobStatus = new()
        {
            JobId = jobId,
            State = PdfJobState.Pending,
            CreatedAt = DateTime.UtcNow,
            PdfCacheKey = pdfCacheKey
        };

        await cacheService.SetAsync(jobStatusKey, jobStatus, TimeSpan.FromHours(1), cancellationToken);

        string serializedModel = JsonSerializer.Serialize(model);
        string capturedTemplateName = templateName;
        string capturedJobId = jobId;
        string capturedJobStatusKey = jobStatusKey;
        string capturedPdfCacheKey = pdfCacheKey;

        backgroundJobService.EnqueueFireAndForget(async ct =>
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                ICacheService scopedCache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                PdfJobStatus updatedStatus = new()
                {
                    JobId = capturedJobId,
                    State = PdfJobState.Processing,
                    CreatedAt = DateTime.UtcNow,
                    PdfCacheKey = capturedPdfCacheKey
                };

                await scopedCache.SetAsync(capturedJobStatusKey, updatedStatus, TimeSpan.FromHours(1), ct);

                logger.LogInformation("Starting PDF generation for job {JobId}, template {Template}", capturedJobId, capturedTemplateName);

                IPdfGenerator pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();

                TModel deserializedModel = JsonSerializer.Deserialize<TModel>(serializedModel)!;
                byte[] pdfBytes = await pdfGenerator.GeneratePdfAsync(capturedTemplateName, deserializedModel, ct);

                await scopedCache.SetAsync(capturedPdfCacheKey, pdfBytes, TimeSpan.FromHours(24), ct);

                updatedStatus.State = PdfJobState.Completed;
                updatedStatus.CompletedAt = DateTime.UtcNow;
                await scopedCache.SetAsync(capturedJobStatusKey, updatedStatus, TimeSpan.FromHours(1), ct);

                logger.LogInformation("PDF generation completed for job {JobId}", capturedJobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PDF generation failed for job {JobId}", capturedJobId);

                using IServiceScope scope = serviceProvider.CreateScope();
                ICacheService scopedCache = scope.ServiceProvider.GetRequiredService<ICacheService>();

                PdfJobStatus failedStatus = new()
                {
                    JobId = capturedJobId,
                    State = PdfJobState.Failed,
                    CreatedAt = DateTime.UtcNow,
                    PdfCacheKey = capturedPdfCacheKey,
                    ErrorMessage = ex.Message
                };

                await scopedCache.SetAsync(capturedJobStatusKey, failedStatus, TimeSpan.FromHours(1), ct);
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
