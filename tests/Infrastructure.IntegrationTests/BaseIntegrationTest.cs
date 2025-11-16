using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Infrastructure.IntegrationTests;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var services = new ServiceCollection();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(_dbContainer.GetConnectionString()));

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            T? result = await action(DbContext);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
