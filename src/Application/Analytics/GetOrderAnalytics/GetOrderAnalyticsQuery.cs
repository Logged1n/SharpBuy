using Application.Abstractions.Messaging;

namespace Application.Analytics.GetOrderAnalytics;

public sealed record GetOrderAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Granularity Granularity) : IQuery<OrderAnalyticsResponse>;
