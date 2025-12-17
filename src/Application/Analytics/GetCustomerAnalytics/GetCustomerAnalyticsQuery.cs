using Application.Abstractions.Messaging;

namespace Application.Analytics.GetCustomerAnalytics;

public sealed record GetCustomerAnalyticsQuery(
    DateTime StartDate,
    DateTime EndDate,
    Granularity Granularity) : IQuery<CustomerAnalyticsResponse>;
