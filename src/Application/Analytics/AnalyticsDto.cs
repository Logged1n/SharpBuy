namespace Application.Analytics;

public sealed record AnalyticsRequest(
    DateTime StartDate,
    DateTime EndDate,
    ReportType ReportType,
    Granularity Granularity);

public enum ReportType
{
    Sales = 0,
    Products = 1,
    Customers = 2,
    Orders = 3
}

public enum Granularity
{
    Day = 0,
    Week = 1,
    Month = 2
}

public sealed record SalesAnalyticsResponse(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    decimal GrowthPercentage,
    List<SalesDataPoint> DataPoints);

public sealed record SalesDataPoint(
    DateTime Date,
    decimal Revenue,
    int OrderCount);

public sealed record ProductAnalyticsResponse(
    int TotalProductsSold,
    decimal TotalRevenue,
    List<ProductPerformance> TopProducts,
    List<ProductPerformance> ProductPerformanceByPeriod);

public sealed record ProductPerformance(
    Guid ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue,
    DateTime? Period);

public sealed record CustomerAnalyticsResponse(
    int TotalCustomers,
    int NewCustomers,
    int ReturningCustomers,
    decimal AverageCustomerValue,
    List<CustomerDataPoint> DataPoints);

public sealed record CustomerDataPoint(
    DateTime Date,
    int NewCustomers,
    int ReturningCustomers);

public sealed record OrderAnalyticsResponse(
    int TotalOrders,
    int CompletedOrders,
    int PendingOrders,
    int CancelledOrders,
    decimal CompletionRate,
    List<OrderDataPoint> DataPoints);

public sealed record OrderDataPoint(
    DateTime Date,
    int Completed,
    int Pending,
    int Cancelled);
