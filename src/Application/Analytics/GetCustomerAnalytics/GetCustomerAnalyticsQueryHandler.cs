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
        // Get all users with their first order date
        List<UserInfo> usersWithOrders = await dbContext.DomainUsers
            .AsNoTracking()
            .Select(u => new UserInfo(
                u.Id,
                dbContext.Orders
                    .Where(o => o.UserId == u.Id)
                    .OrderBy(o => o.CreatedAt)
                    .Select(o => o.CreatedAt)
                    .FirstOrDefault(),
                dbContext.Orders
                    .Where(o => o.UserId == u.Id && o.CreatedAt >= query.StartDate && o.CreatedAt <= query.EndDate)
                    .SelectMany(o => o.Items)
                    .Sum(oi => oi.TotalPrice.Amount)))
            .Where(u => u.FirstOrderDate != default)
            .ToListAsync(cancellationToken);

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
