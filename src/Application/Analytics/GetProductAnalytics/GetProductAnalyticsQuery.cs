using Application.Abstractions.Messaging;

namespace Application.Analytics.GetProductAnalytics;

public sealed record GetProductAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Granularity Granularity) : IQuery<ProductAnalyticsResponse>;
