# SharpBuy Performance Benchmarks

This project contains performance benchmarks for critical SharpBuy operations using BenchmarkDotNet.

## Running Benchmarks

To run the benchmarks in Release mode (recommended):

```bash
cd tests/Performance.Benchmarks

# Run cart benchmarks (default - measures requests per second)
dotnet run -c Release -- --cart

# Run analytics benchmarks
dotnet run -c Release -- --analytics

# Run all benchmarks
dotnet run -c Release
```

## Benchmark Scenarios

### Cart Benchmarks (Throughput/Requests per Second)

Measures how many add-to-cart operations can be handled per second:

- **AddItemToCart_Single**: Single add-to-cart operation performance
- **AddItemToCart_Anonymous**: Anonymous user cart validation (client-side cart)
- **AddItemToCart_Concurrent10**: 10 concurrent requests
- **AddItemToCart_Concurrent50**: 50 concurrent requests (stress test)
- **AddItemToCart_Concurrent100**: 100 concurrent requests (high load)
- **AddMultipleItems_Sequential**: Adding 10 different items sequentially

**Why this matters**: Add-to-cart is one of the most frequent operations in e-commerce. These benchmarks help determine:
- Maximum requests per second the system can handle
- How the system performs under concurrent load
- Memory allocation patterns
- Scalability limits

### Analytics Benchmarks

Individual analytics queries:

- **GetSalesAnalytics_Day**: Measures performance of sales analytics with daily granularity
- **GetProductAnalytics_Day**: Measures performance of product analytics with daily granularity
- **GetCustomerAnalytics_Day**: Measures performance of customer analytics with daily granularity
- **GetOrderAnalytics_Day**: Measures performance of order analytics with daily granularity

Aggregated scenarios:

- **GetAllAnalytics_Sequential**: Runs all 4 analytics queries sequentially
- **GetAllAnalytics_Parallel**: Runs all 4 analytics queries in parallel using `Task.WhenAll`

## Understanding Results

BenchmarkDotNet will output:

- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation of all measurements
- **Gen0/Gen1/Gen2**: Number of garbage collections per 1000 operations
- **Allocated**: Total memory allocated

## Sample Output

### Cart Benchmarks (Requests per Second)

```
| Method                                   | Mean      | Min       | Max       | Median    | Allocated |
|----------------------------------------- |----------:|----------:|----------:|----------:|----------:|
| AddItemToCart_Single                     |  1.234 ms |  1.156 ms |  1.398 ms |  1.221 ms |   15.2 KB |
| AddItemToCart_Anonymous                  |  0.456 ms |  0.421 ms |  0.512 ms |  0.449 ms |    5.8 KB |
| AddItemToCart_Concurrent10               |  2.567 ms |  2.334 ms |  2.891 ms |  2.543 ms |  152.4 KB |
| AddItemToCart_Concurrent50               | 12.891 ms | 11.234 ms | 14.567 ms | 12.765 ms |  761.2 KB |
| AddItemToCart_Concurrent100              | 25.432 ms | 23.112 ms | 28.891 ms | 25.234 ms | 1523.6 KB |
| AddMultipleItems_Sequential              | 12.345 ms | 11.567 ms | 13.234 ms | 12.289 ms |  152.0 KB |
```

**Calculating Requests/Second**:
- Single operation: ~810 req/s (1000ms / 1.234ms)
- 10 concurrent: ~3,895 req/s (10 / 2.567ms * 1000)
- 50 concurrent: ~3,880 req/s (50 / 12.891ms * 1000)
- 100 concurrent: ~3,932 req/s (100 / 25.432ms * 1000)

### Analytics Benchmarks

```
| Method                           | Mean      | Error    | StdDev   | Gen0   | Allocated |
|--------------------------------- |----------:|---------:|---------:|-------:|----------:|
| GetSalesAnalytics_Day           |  50.23 μs | 0.987 μs | 0.924 μs | 0.0610 |   1.01 KB |
| GetProductAnalytics_Day         |  45.67 μs | 0.654 μs | 0.612 μs | 0.0610 |   0.98 KB |
| GetCustomerAnalytics_Day        |  48.91 μs | 0.821 μs | 0.768 μs | 0.0610 |   0.99 KB |
| GetOrderAnalytics_Day           |  42.34 μs | 0.743 μs | 0.695 μs | 0.0610 |   0.95 KB |
| GetAllAnalytics_Sequential      | 187.15 μs | 2.456 μs | 2.297 μs | 0.2441 |   3.93 KB |
| GetAllAnalytics_Parallel        |  52.89 μs | 1.023 μs | 0.957 μs | 0.1221 |   2.12 KB |
```

## Notes

- Cart benchmarks use Testcontainers with real PostgreSQL database for realistic testing
- Analytics benchmarks use an in-memory database for fast execution
- **Docker must be running** to execute cart benchmarks (Testcontainers requirement)
- Results may vary based on hardware and system load
- Always run benchmarks in Release mode for accurate results
- Parallel execution shows significant performance improvement for aggregated queries
- Concurrent benchmarks use scoped DbContext instances to simulate real API behavior
