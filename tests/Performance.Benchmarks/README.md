# SharpBuy Analytics Performance Benchmarks

This project contains performance benchmarks for the analytics endpoints using BenchmarkDotNet.

## Running Benchmarks

To run the benchmarks in Release mode (recommended):

```bash
cd tests/Performance.Benchmarks
dotnet run -c Release
```

## Benchmark Scenarios

The benchmarks test the following scenarios:

### Individual Analytics Queries

-  **GetSalesAnalytics_Day**: Measures performance of sales analytics with daily granularity
- **GetProductAnalytics_Day**: Measures performance of product analytics with daily granularity
- **GetCustomerAnalytics_Day**: Measures performance of customer analytics with daily granularity
- **GetOrderAnalytics_Day**: Measures performance of order analytics with daily granularity

### Aggregated Scenarios

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

- Benchmarks use an in-memory database for consistent, isolated testing
- Results may vary based on hardware and system load
- Always run benchmarks in Release mode for accurate results
- Parallel execution shows significant performance improvement for aggregated queries
