using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Analytics.GetProductAnalytics;

internal sealed class GetProductAnalyticsQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetProductAnalyticsQuery, ProductAnalyticsResponse>
{
    public async Task<Result<ProductAnalyticsResponse>> Handle(
        GetProductAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        // Fetch orders with their items in a single query
        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= query.StartDate && o.CreatedAt <= query.EndDate)
            .Select(o => new
            {
                o.CreatedAt,
                Items = o.Items.Select(oi => new
                {
                    oi.ProductId,
                    oi.Quantity,
                    TotalAmount = oi.TotalPrice.Amount
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        // Flatten to OrderItemInfo in memory
        var orderItems = orders
            .SelectMany(o => o.Items.Select(oi => new OrderItemInfo(
                oi.ProductId,
                o.CreatedAt,
                oi.Quantity,
                oi.TotalAmount)))
            .ToList();

        if (orderItems.Count == 0)
        {
            return new ProductAnalyticsResponse(
                TotalProductsSold: 0,
                TotalRevenue: 0,
                TopProducts: [],
                ProductPerformanceByPeriod: []);
        }

        int totalProductsSold = orderItems.Sum(oi => oi.Quantity);
        decimal totalRevenue = orderItems.Sum(oi => oi.Revenue);

        // Get product names
        var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();
        Dictionary<Guid, string> products = await dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        // Top products overall
        var topProducts = orderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new ProductPerformance(
                ProductId: g.Key,
                ProductName: products.GetValueOrDefault(g.Key, "Unknown"),
                QuantitySold: g.Sum(oi => oi.Quantity),
                Revenue: g.Sum(oi => oi.Revenue),
                Period: null))
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        // Performance by period
        List<ProductPerformance> performanceByPeriod = query.Granularity switch
        {
            Granularity.Day => GroupByDay(orderItems, products, query.StartDate, query.EndDate),
            Granularity.Week => GroupByWeek(orderItems, products, query.StartDate, query.EndDate),
            Granularity.Month => GroupByMonth(orderItems, products, query.StartDate, query.EndDate),
            _ => GroupByDay(orderItems, products, query.StartDate, query.EndDate)
        };

        return new ProductAnalyticsResponse(
            TotalProductsSold: totalProductsSold,
            TotalRevenue: totalRevenue,
            TopProducts: topProducts,
            ProductPerformanceByPeriod: performanceByPeriod);
    }

    private static List<ProductPerformance> GroupByDay(
        List<OrderItemInfo> orderItems,
        Dictionary<Guid, string> products,
        DateTime start,
        DateTime end)
    {
        var performance = new List<ProductPerformance>();
        for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var dayItems = orderItems.Where(oi => oi.CreatedAt.Date == date).ToList();
            IEnumerable<IGrouping<Guid, OrderItemInfo>> grouped = dayItems.GroupBy(oi => oi.ProductId);
            foreach (IGrouping<Guid, OrderItemInfo> group in grouped)
            {
                performance.Add(new ProductPerformance(
                    ProductId: group.Key,
                    ProductName: products.GetValueOrDefault(group.Key, "Unknown"),
                    QuantitySold: group.Sum(oi => oi.Quantity),
                    Revenue: group.Sum(oi => oi.Revenue),
                    Period: date));
            }
        }
        return performance;
    }

    private static List<ProductPerformance> GroupByWeek(
        List<OrderItemInfo> orderItems,
        Dictionary<Guid, string> products,
        DateTime start,
        DateTime end)
    {
        var performance = new List<ProductPerformance>();
        DateTime current = start.Date;
        while (current <= end)
        {
            DateTime weekEnd = current.AddDays(7);
            var weekItems = orderItems.Where(oi => oi.CreatedAt >= current && oi.CreatedAt < weekEnd).ToList();
            IEnumerable<IGrouping<Guid, OrderItemInfo>> grouped = weekItems.GroupBy(oi => oi.ProductId);
            foreach (IGrouping<Guid, OrderItemInfo> group in grouped)
            {
                performance.Add(new ProductPerformance(
                    ProductId: group.Key,
                    ProductName: products.GetValueOrDefault(group.Key, "Unknown"),
                    QuantitySold: group.Sum(oi => oi.Quantity),
                    Revenue: group.Sum(oi => oi.Revenue),
                    Period: current));
            }
            current = weekEnd;
        }
        return performance;
    }

    private static List<ProductPerformance> GroupByMonth(
        List<OrderItemInfo> orderItems,
        Dictionary<Guid, string> products,
        DateTime start,
        DateTime end)
    {
        var performance = new List<ProductPerformance>();
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (current <= end)
        {
            DateTime monthEnd = current.AddMonths(1);
            var monthItems = orderItems.Where(oi => oi.CreatedAt >= current && oi.CreatedAt < monthEnd).ToList();
            IEnumerable<IGrouping<Guid, OrderItemInfo>> grouped = monthItems.GroupBy(oi => oi.ProductId);
            foreach (IGrouping<Guid, OrderItemInfo> group in grouped)
            {
                performance.Add(new ProductPerformance(
                    ProductId: group.Key,
                    ProductName: products.GetValueOrDefault(group.Key, "Unknown"),
                    QuantitySold: group.Sum(oi => oi.Quantity),
                    Revenue: group.Sum(oi => oi.Revenue),
                    Period: current));
            }
            current = monthEnd;
        }
        return performance;
    }

    private sealed record OrderItemInfo(Guid ProductId, DateTime CreatedAt, int Quantity, decimal Revenue);
}
