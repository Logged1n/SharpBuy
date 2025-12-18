namespace Application.Abstractions.Reporting;

public sealed class PdfJobStatus
{
    public string JobId { get; set; } = string.Empty;
    public PdfJobState State { get; set; }
    public string? PdfCacheKey { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum PdfJobState
{
    Pending,
    Processing,
    Completed,
    Failed
}
