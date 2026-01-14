using Application.Abstractions.Caching;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class QueryCachingDecorator<TQuery, TResponse>(
    IQueryHandler<TQuery, TResponse> handler,
    ICacheService cacheService,
    ILogger<QueryCachingDecorator<TQuery, TResponse>> logger)
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        // Only apply caching if the query implements ICacheableQuery
        if (query is not ICacheableQuery<TResponse> cacheableQuery)
        {
            return await handler.Handle(query, cancellationToken);
        }

        string cacheKey = cacheableQuery.CacheKey;

        // Try to get from cache
        TResponse? cachedResult = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResult is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return Result.Success(cachedResult);
        }

        logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Execute the handler
        Result<TResponse> result = await handler.Handle(query, cancellationToken);

        // Cache the result if successful
        if (result.IsSuccess)
        {
            await cacheService.SetAsync(cacheKey, result.Value, cacheableQuery.CacheExpiration, cancellationToken);
            logger.LogDebug("Cached result for key: {CacheKey} with expiration: {Expiration}", cacheKey, cacheableQuery.CacheExpiration);
        }

        return result;
    }
}
