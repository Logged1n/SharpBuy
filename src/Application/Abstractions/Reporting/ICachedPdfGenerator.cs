namespace Application.Abstractions.Reporting;

public interface ICachedPdfGenerator
{
    Task<string> GeneratePdfAsync<TModel>(
        string templateName,
        TModel model,
        string cacheKey,
        CancellationToken cancellationToken = default);

    Task<PdfJobStatus?> GetJobStatusAsync(string jobId, CancellationToken cancellationToken = default);

    Task<byte[]?> GetPdfAsync(string pdfCacheKey, CancellationToken cancellationToken = default);
}
