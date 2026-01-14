namespace Application.Abstractions.Caching;

/// <summary>
/// Provides methods to invalidate cache entries.
/// </summary>
public interface ICacheInvalidator
{
    /// <summary>
    /// Invalidates all cache entries that match the pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match cache keys (e.g., "product_*")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a specific cache entry.
    /// </summary>
    /// <param name="key">The cache key to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates multiple cache entries.
    /// </summary>
    /// <param name="keys">The cache keys to invalidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InvalidateMultipleAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
}
