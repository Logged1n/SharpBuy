using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Analytics.GetCustomerAnalytics;

internal sealed class GetCustomerAnalyticsQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsResponse>
{
    public async Task<Result<CustomerAnalyticsResponse>> Handle(
        GetCustomerAnalyticsQuery query,
        CancellationToken cancellationToken)
    {
        // Get all user IDs that have orders
        List<Guid> userIdsWithOrders = await dbContext.Orders
            .AsNoTracking()
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (userIdsWithOrders.Count == 0)
        {
            return new CustomerAnalyticsResponse(
                TotalCustomers: 0,
                NewCustomers: 0,
                ReturningCustomers: 0,
                AverageCustomerValue: 0,
                DataPoints: []);
        }

        // Get first order dates for all users
        var firstOrderDates = await dbContext.Orders
            .AsNoTracking()
            .Where(o => userIdsWithOrders.Contains(o.UserId))
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                FirstOrderDate = g.Min(o => o.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        // Calculate total spent per user in the date range
        var userSpending = await dbContext.Orders
            .AsNoTracking()
            .Where(o => userIdsWithOrders.Contains(o.UserId) &&
                       o.CreatedAt >= query.StartDate &&
                       o.CreatedAt <= query.EndDate)
            .SelectMany(o => o.Items.Select(oi => new { o.UserId, oi.TotalPrice }))
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalSpent = g.Sum(x => x.TotalPrice.Amount)
            })
            .ToListAsync(cancellationToken);

        // Combine the data
        var usersWithOrders = firstOrderDates.Select(fod => new UserInfo(
            fod.UserId,
            fod.FirstOrderDate,
            userSpending.FirstOrDefault(us => us.UserId == fod.UserId)?.TotalSpent ?? 0m
        )).ToList();

        int totalCustomers = usersWithOrders.Count;
        var newCustomers = usersWithOrders
            .Where(u => u.FirstOrderDate >= query.StartDate && u.FirstOrderDate <= query.EndDate)
            .ToList();
        int newCustomerCount = newCustomers.Count;
        int returningCustomerCount = totalCustomers - newCustomerCount;

        decimal averageCustomerValue = usersWithOrders.Count != 0
            ? usersWithOrders.Average(u => u.TotalSpent)
            : 0;

        // Group by granularity
        List<CustomerDataPoint> dataPoints = query.Granularity switch
        {
            Granularity.Day => GroupByDay(usersWithOrders, query.StartDate, query.EndDate),
            Granularity.Week => GroupByWeek(usersWithOrders, query.StartDate, query.EndDate),
            Granularity.Month => GroupByMonth(usersWithOrders, query.StartDate, query.EndDate),
            _ => GroupByDay(usersWithOrders, query.StartDate, query.EndDate)
        };

        return new CustomerAnalyticsResponse(
            TotalCustomers: totalCustomers,
            NewCustomers: newCustomerCount,
            ReturningCustomers: returningCustomerCount,
            AverageCustomerValue: averageCustomerValue,
            DataPoints: dataPoints);
    }

    private static List<CustomerDataPoint> GroupByDay(List<UserInfo> users, DateTime start, DateTime end)
    {
        var dataPoints = new List<CustomerDataPoint>();
        for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
        {
            var newCustomers = users.Where(u => u.FirstOrderDate.Date == date).ToList();
            var returningCustomers = users.Where(u =>
                u.FirstOrderDate.Date < date &&
                u.TotalSpent > 0).ToList();

            dataPoints.Add(new CustomerDataPoint(
                Date: date,
                NewCustomers: newCustomers.Count,
                ReturningCustomers: returningCustomers.Count));
        }
        return dataPoints;
    }

    private static List<CustomerDataPoint> GroupByWeek(List<UserInfo> users, DateTime start, DateTime end)
    {
        var dataPoints = new List<CustomerDataPoint>();
        DateTime current = start.Date;
        while (current <= end)
        {
            DateTime weekEnd = current.AddDays(7);
            var newCustomers = users.Where(u =>
                u.FirstOrderDate >= current &&
                u.FirstOrderDate < weekEnd).ToList();
            var returningCustomers = users.Where(u =>
                u.FirstOrderDate < current &&
                u.TotalSpent > 0).ToList();

            dataPoints.Add(new CustomerDataPoint(
                Date: current,
                NewCustomers: newCustomers.Count,
                ReturningCustomers: returningCustomers.Count));
            current = weekEnd;
        }
        return dataPoints;
    }

    private static List<CustomerDataPoint> GroupByMonth(List<UserInfo> users, DateTime start, DateTime end)
    {
        var dataPoints = new List<CustomerDataPoint>();
        var current = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (current <= end)
        {
            DateTime monthEnd = current.AddMonths(1);
            var newCustomers = users.Where(u =>
                u.FirstOrderDate >= current &&
                u.FirstOrderDate < monthEnd).ToList();
            var returningCustomers = users.Where(u =>
                u.FirstOrderDate < current &&
                u.TotalSpent > 0).ToList();

            dataPoints.Add(new CustomerDataPoint(
                Date: current,
                NewCustomers: newCustomers.Count,
                ReturningCustomers: returningCustomers.Count));
            current = monthEnd;
        }
        return dataPoints;
    }

    private sealed record UserInfo(Guid Id, DateTime FirstOrderDate, decimal TotalSpent);
}
