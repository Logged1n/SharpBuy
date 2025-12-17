using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Analytics.GetSalesAnalytics;

internal sealed class GetSalesAnalyticsQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetSalesAnalyticsQuery, SalesAnalyticsResponse>
{
    public async Task<Result<SalesAnalyticsResponse>> Handle(
        GetSalesAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        List<Domain.Orders.Order> orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= query.StartDate && o.CreatedAt <= query.EndDate)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return new SalesAnalyticsResponse(
                TotalRevenue: 0,
                TotalOrders: 0,
                AverageOrderValue: 0,
                GrowthPercentage: 0,
                DataPoints: []);
        }

        decimal totalRevenue = orders.Sum(o => o.Items.Sum(oi => oi.TotalPrice.Amount));
        int totalOrders = orders.Count;
        decimal averageOrderValue = totalRevenue / totalOrders;

        // Calculate growth percentage (compare with previous period)
        TimeSpan periodLength = query.EndDate - query.StartDate;
        DateTime previousPeriodStart = query.StartDate.AddDays(-periodLength.Days);
        List<Domain.Orders.Order> previousOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= previousPeriodStart && o.CreatedAt < query.StartDate)
            .Include(o => o.Items)
            .ToListAsync(cancellationToken);

        decimal previousRevenue = previousOrders.Sum(o => o.Items.Sum(oi => oi.TotalPrice.Amount));
        decimal growthPercentage = previousRevenue > 0
            ? (totalRevenue - previousRevenue) / previousRevenue * 100
            : 0;

        // Group by granularity
        List<SalesDataPoint> dataPoints = query.Granularity switch
        {
            Granularity.Day => GroupByDay(orders, query.StartDate, query.EndDate),
            Granularity.Week => GroupByWeek(orders, query.StartDate, query.EndDate),
            Granularity.Month => GroupByMonth(orders, query.StartDate, query.EndDate),
            _ => GroupByDay(orders, query.StartDate, query.EndDate)
        };

        return new SalesAnalyticsResponse(
            TotalRevenue: totalRevenue,
            TotalOrders: totalOrders,
            AverageOrderValue: averageOrderValue,
            GrowthPercentage: growthPercentage,
            DataPoints: dataPoints);
    }

    private static List<SalesDataPoint> GroupByDay(List<Domain.Orders.Order> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<SalesDataPoint>();
        for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
            dataPoints.Add(new SalesDataPoint(
                Date: date,
                Revenue: dayOrders.Sum(o => o.Items.Sum(oi => oi.TotalPrice.Amount)),
                OrderCount: dayOrders.Count));
        }
        return dataPoints;
    }

    private static List<SalesDataPoint> GroupByWeek(List<Domain.Orders.Order> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<SalesDataPoint>();
        DateTime current = start.Date;
        while (current <= end)
        {
            DateTime weekEnd = current.AddDays(7);
            var weekOrders = orders.Where(o => o.CreatedAt >= current && o.CreatedAt < weekEnd).ToList();
            dataPoints.Add(new SalesDataPoint(
                Date: current,
                Revenue: weekOrders.Sum(o => o.Items.Sum(oi => oi.TotalPrice.Amount)),
                OrderCount: weekOrders.Count));
            current = weekEnd;
        }
        return dataPoints;
    }

    private static List<SalesDataPoint> GroupByMonth(List<Domain.Orders.Order> orders, DateTime start, DateTime end)
    {
        var dataPoints = new List<SalesDataPoint>();
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (current <= end)
        {
            DateTime monthEnd = current.AddMonths(1);
            var monthOrders = orders.Where(o => o.CreatedAt >= current && o.CreatedAt < monthEnd).ToList();
            dataPoints.Add(new SalesDataPoint(
                Date: current,
                Revenue: monthOrders.Sum(o => o.Items.Sum(oi => oi.TotalPrice.Amount)),
                OrderCount: monthOrders.Count));
            current = monthEnd;
        }
        return dataPoints;
    }
}
