using Application.Carts.AddItem;
using BenchmarkDotNet.Attributes;
using Domain.Carts;
using Domain.Categories;
using Domain.Products;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.ValueObjects;
using Testcontainers.PostgreSql;

namespace Performance.Benchmarks;

/// <summary>
/// Benchmark for cart operations to measure throughput (requests per second).
/// Shows how many add-to-cart operations the system can handle per second.
/// Uses Testcontainers with real PostgreSQL database for realistic testing.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class CartBenchmarks
{
    private PostgreSqlContainer _postgresContainer = null!;
    private IServiceProvider _serviceProvider = null!;
    private ApplicationDbContext _dbContext = null!;
    private Guid _userId;
    private Guid _productId;
    private AddItemToCartCommand _addItemCommand = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("SharpBuyBenchmark")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        // Configure services with real PostgreSQL
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(_postgresContainer.GetConnectionString());
            options.UseSnakeCaseNamingConvention();
        });

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Create database schema
        _dbContext.Database.EnsureCreated();

        // Setup test data
        SeedData();
    }

    private void SeedData()
    {
        // Create category
        var category = Category.Create("Electronics");
        _dbContext.Categories.Add(category);
        _dbContext.SaveChanges();

        // Create product with inventory
        var product = Product.Create(
            "Gaming Mouse",
            "High-performance gaming mouse",
            quantity: 1000,
            priceAmount: 59.99m,
            priceCurrency: "USD",
            mainPhotoPath: "/photos/mouse.jpg");
        product.AddToCategory(category.Id);

        _dbContext.Products.Add(product);
        _dbContext.SaveChanges();

        _productId = product.Id;

        // Create user with cart
        var user = DomainUser.Create(
            "benchmark@test.com",
            "Benchmark",
            "User",
            "123456789");

        _dbContext.DomainUsers.Add(user);
        _dbContext.SaveChanges();

        _userId = user.Id;

        // Setup command
        _addItemCommand = new AddItemToCartCommand(_userId, _productId, 1);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _dbContext?.Dispose();
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_postgresContainer is not null)
        {
            _postgresContainer.StopAsync().GetAwaiter().GetResult();
            _postgresContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Clear cart items before each iteration to avoid duplicates
        Cart cart = _dbContext.Carts
            .Include(c => c.Items)
            .First(c => c.OwnerId == _userId);

        _dbContext.CartItems.RemoveRange(cart.Items);
        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Benchmark: Single add-to-cart operation.
    /// Measures how fast a single request can be processed.
    /// </summary>
    [Benchmark(Description = "Add item to cart - single operation")]
    public async Task AddItemToCart_Single()
    {
        var handler = new AddItemToCartCommandHandler(_dbContext);
        await handler.Handle(_addItemCommand, CancellationToken.None);
    }

    /// <summary>
    /// Benchmark: Add item for anonymous user (validation only).
    /// Shows the performance when cart is managed client-side.
    /// </summary>
    [Benchmark(Description = "Add item to cart - anonymous user (validation only)")]
    public async Task AddItemToCart_Anonymous()
    {
        var command = new AddItemToCartCommand(null, _productId, 1);
        var handler = new AddItemToCartCommandHandler(_dbContext);
        await handler.Handle(command, CancellationToken.None);
    }

    /// <summary>
    /// Benchmark: Multiple concurrent add-to-cart operations.
    /// Simulates 10 concurrent requests to measure throughput.
    /// Each request uses its own DbContext to simulate real API behavior.
    /// </summary>
    [Benchmark(Description = "Add item to cart - 10 concurrent operations")]
    public async Task AddItemToCart_Concurrent10()
    {
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                AddItemToCartCommandHandler handler = new(dbContext);
                await handler.Handle(_addItemCommand, CancellationToken.None);
            });
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: High concurrency scenario.
    /// Simulates 50 concurrent requests to test system limits.
    /// Each request uses its own DbContext to simulate real API behavior.
    /// </summary>
    [Benchmark(Description = "Add item to cart - 50 concurrent operations")]
    public async Task AddItemToCart_Concurrent50()
    {
        var tasks = new Task[50];
        for (int i = 0; i < 50; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                AddItemToCartCommandHandler handler = new(dbContext);
                await handler.Handle(_addItemCommand, CancellationToken.None);
            });
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: Very high concurrency scenario.
    /// Simulates 100 concurrent requests for stress testing.
    /// Each request uses its own DbContext to simulate real API behavior.
    /// </summary>
    [Benchmark(Description = "Add item to cart - 100 concurrent operations")]
    public async Task AddItemToCart_Concurrent100()
    {
        var tasks = new Task[100];
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                AddItemToCartCommandHandler handler = new(dbContext);
                await handler.Handle(_addItemCommand, CancellationToken.None);
            });
        }
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: Adding multiple different products sequentially.
    /// Measures the performance of adding 10 different items.
    /// </summary>
    [Benchmark(Description = "Add 10 different items sequentially")]
    public async Task AddMultipleItems_Sequential()
    {
        var handler = new AddItemToCartCommandHandler(_dbContext);
        for (int i = 0; i < 10; i++)
        {
            var command = new AddItemToCartCommand(_userId, _productId, 1);
            await handler.Handle(command, CancellationToken.None);
        }
    }
}
