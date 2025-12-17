using Application.Analytics;
using Application.Analytics.GetSalesAnalytics;
using Application.Analytics.GetProductAnalytics;
using Application.Analytics.GetCustomerAnalytics;
using Application.Analytics.GetOrderAnalytics;
using BenchmarkDotNet.Attributes;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Performance.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class AnalyticsBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _dbContext = null!;
    private GetSalesAnalyticsQuery _salesQuery = null!;
    private GetProductAnalyticsQuery _productQuery = null!;
    private GetCustomerAnalyticsQuery _customerQuery = null!;
    private GetOrderAnalyticsQuery _orderQuery = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        ServiceCollection services = new();

        // Configure in-memory database for benchmarking
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("BenchmarkDb"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Setup test queries
        DateTime endDate = DateTime.UtcNow;
        DateTime startDate = endDate.AddDays(-30);

        _salesQuery = new GetSalesAnalyticsQuery(startDate, endDate, Granularity.Day);
        _productQuery = new GetProductAnalyticsQuery(startDate, endDate, Granularity.Day);
        _customerQuery = new GetCustomerAnalyticsQuery(startDate, endDate, Granularity.Day);
        _orderQuery = new GetOrderAnalyticsQuery(startDate, endDate, Granularity.Day);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _dbContext?.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Benchmark]
    public async Task GetSalesAnalytics_Day()
    {
        var handler = new GetSalesAnalyticsQueryHandler(_dbContext);
        await handler.Handle(_salesQuery, CancellationToken.None);
    }

    [Benchmark]
    public async Task GetProductAnalytics_Day()
    {
        var handler = new GetProductAnalyticsQueryHandler(_dbContext);
        await handler.Handle(_productQuery, CancellationToken.None);
    }

    [Benchmark]
    public async Task GetCustomerAnalytics_Day()
    {
        var handler = new GetCustomerAnalyticsQueryHandler(_dbContext);
        await handler.Handle(_customerQuery, CancellationToken.None);
    }

    [Benchmark]
    public async Task GetOrderAnalytics_Day()
    {
        var handler = new GetOrderAnalyticsQueryHandler(_dbContext);
        await handler.Handle(_orderQuery, CancellationToken.None);
    }

    [Benchmark]
    public async Task GetAllAnalytics_Sequential()
    {
        var salesHandler = new GetSalesAnalyticsQueryHandler(_dbContext);
        var productHandler = new GetProductAnalyticsQueryHandler(_dbContext);
        var customerHandler = new GetCustomerAnalyticsQueryHandler(_dbContext);
        var orderHandler = new GetOrderAnalyticsQueryHandler(_dbContext);

        await salesHandler.Handle(_salesQuery, CancellationToken.None);
        await productHandler.Handle(_productQuery, CancellationToken.None);
        await customerHandler.Handle(_customerQuery, CancellationToken.None);
        await orderHandler.Handle(_orderQuery, CancellationToken.None);
    }

    [Benchmark]
    public async Task GetAllAnalytics_Parallel()
    {
        var salesHandler = new GetSalesAnalyticsQueryHandler(_dbContext);
        var productHandler = new GetProductAnalyticsQueryHandler(_dbContext);
        var customerHandler = new GetCustomerAnalyticsQueryHandler(_dbContext);
        var orderHandler = new GetOrderAnalyticsQueryHandler(_dbContext);

        await Task.WhenAll(
            salesHandler.Handle(_salesQuery, CancellationToken.None),
            productHandler.Handle(_productQuery, CancellationToken.None),
            customerHandler.Handle(_customerQuery, CancellationToken.None),
            orderHandler.Handle(_orderQuery, CancellationToken.None)
        );
    }
}
