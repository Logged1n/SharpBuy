namespace Application.Abstractions.Messaging;

/// <summary>
/// Represents a query that can be cached.
/// </summary>
/// <typeparam name="TResponse">The query response type</typeparam>
public interface ICacheableQuery<TResponse> : IQuery<TResponse>
{
    /// <summary>
    /// Gets the cache key for this query.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets the cache expiration time. If null, uses default expiration.
    /// </summary>
    TimeSpan? CacheExpiration { get; }
}
