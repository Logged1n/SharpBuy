using BenchmarkDotNet.Running;
using Performance.Benchmarks;

#pragma warning disable CA1303 // Do not pass literals as localized parameters

string[] commandLineArgs = Environment.GetCommandLineArgs();
if (commandLineArgs.Length > 1 && commandLineArgs.Contains("--cart"))
{
    Console.WriteLine("Running Cart Benchmarks...");
    BenchmarkRunner.Run<CartBenchmarks>();
}
else if (commandLineArgs.Length > 1 && commandLineArgs.Contains("--analytics"))
{
    Console.WriteLine("Running Analytics Benchmarks...");
    BenchmarkRunner.Run<AnalyticsBenchmarks>();
}
else
{
    Console.WriteLine("Running all benchmarks...");
    Console.WriteLine();
    Console.WriteLine("Available benchmarks:");
    Console.WriteLine("  1. Cart Benchmarks (add-to-cart throughput)");
    Console.WriteLine("  2. Analytics Benchmarks");
    Console.WriteLine();
    Console.WriteLine("To run specific benchmark:");
    Console.WriteLine("  dotnet run -c Release -- --cart");
    Console.WriteLine("  dotnet run -c Release -- --analytics");
    Console.WriteLine();

    // Run Cart benchmarks by default (most relevant for e-commerce performance)
    BenchmarkRunner.Run<CartBenchmarks>();
}
