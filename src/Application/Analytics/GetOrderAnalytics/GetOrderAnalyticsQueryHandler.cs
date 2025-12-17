using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Analytics.GetOrderAnalytics;

internal sealed class GetOrderAnalyticsQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetOrderAnalyticsQuery, OrderAnalyticsResponse>
{
    public async Task<Result<OrderAnalyticsResponse>> Handle(
        GetOrderAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        List<OrderInfo> orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= query.StartDate && o.CreatedAt <= query.EndDate)
            .Select(o => new OrderInfo(o.CreatedAt, o.Status))
            .ToListAsync(cancellationToken);

        int totalOrders = orders.Count;
        int completedOrders = orders.Count(o => o.Status == OrderStatus.Completed);
        int openOrders = orders.Count(o => o.Status == OrderStatus.Open);
        int cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);

        decimal completionRate = totalOrders > 0
            ? (decimal)completedOrders / totalOrders * 100
            : 0;

        // Group by granularity
        List<OrderDataPoint> dataPoints = query.Granularity switch
        {
            Granularity.Day => GroupByDay(orders, query.StartDate, query.EndDate),
            Granularity.Week => GroupByWeek(orders, query.StartDate, query.EndDate),
            Granularity.Month => GroupByMonth(orders, query.StartDate, query.EndDate),
            _ => GroupByDay(orders, query.StartDate, query.EndDate)
        };

        return new OrderAnalyticsResponse(
            TotalOrders: totalOrders,
            CompletedOrders: completedOrders,
            PendingOrders: openOrders,
            CancelledOrders: cancelledOrders,
            CompletionRate: completionRate,
            DataPoints: dataPoints);
    }

    private static List<OrderDataPoint> GroupByDay(List<OrderInfo> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<OrderDataPoint>();
        for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
            dataPoints.Add(new OrderDataPoint(
                Date: date,
                Completed: dayOrders.Count(o => o.Status == OrderStatus.Completed),
                Pending: dayOrders.Count(o => o.Status == OrderStatus.Open),
                Cancelled: dayOrders.Count(o => o.Status == OrderStatus.Cancelled)));
        }
        return dataPoints;
    }

    private static List<OrderDataPoint> GroupByWeek(List<OrderInfo> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<OrderDataPoint>();
        DateTime current = start.Date;
        while (current <= end)
        {
            DateTime weekEnd = current.AddDays(7);
            var weekOrders = orders.Where(o => o.CreatedAt >= current && o.CreatedAt < weekEnd).ToList();
            dataPoints.Add(new OrderDataPoint(
                Date: current,
                Completed: weekOrders.Count(o => o.Status == OrderStatus.Completed),
                Pending: weekOrders.Count(o => o.Status == OrderStatus.Open),
                Cancelled: weekOrders.Count(o => o.Status == OrderStatus.Cancelled)));
            current = weekEnd;
        }
        return dataPoints;
    }

    private static List<OrderDataPoint> GroupByMonth(List<OrderInfo> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<OrderDataPoint>();
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (current <= end)
        {
            DateTime monthEnd = current.AddMonths(1);
            var monthOrders = orders.Where(o => o.CreatedAt >= current && o.CreatedAt < monthEnd).ToList();
            dataPoints.Add(new OrderDataPoint(
                Date: current,
                Completed: monthOrders.Count(o => o.Status == OrderStatus.Completed),
                Pending: monthOrders.Count(o => o.Status == OrderStatus.Open),
                Cancelled: monthOrders.Count(o => o.Status == OrderStatus.Cancelled)));
            current = monthEnd;
        }
        return dataPoints;
    }

    private sealed record OrderInfo(DateTime CreatedAt, OrderStatus Status);
}
