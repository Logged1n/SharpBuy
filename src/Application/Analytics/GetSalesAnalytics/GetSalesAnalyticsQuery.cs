using Application.Abstractions.Messaging;

namespace Application.Analytics.GetSalesAnalytics;

public sealed record GetSalesAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Granularity Granularity) : IQuery<SalesAnalyticsResponse>;
