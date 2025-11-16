using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Infrastructure.IntegrationTests;

/// <summary>
/// Fixture for managing PostgreSQL test container and database lifecycle
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("SharpBuyTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        // Apply migrations
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention());

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup between tests
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_connection);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options, new NoOpDomainEventDispatcher());
    }
}

/// <summary>
/// No-op domain event dispatcher for testing
/// </summary>
public class NoOpDomainEventDispatcher : Infrastructure.DomainEvents.IDomainEventDispatcher
{
    public Task DispatchAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        // Clear domain events without dispatching
        foreach (var entry in context.ChangeTracker.Entries<SharedKernel.Entity>())
        {
            entry.Entity.ClearDomainEvents();
        }
        return Task.CompletedTask;
    }
}
