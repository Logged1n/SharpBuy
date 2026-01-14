using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Web.Api.IntegrationTests;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("SharpBuyTest")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private IServiceScope _scope = null!;
    protected HttpClient HttpClient { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Konfiguracja WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = _dbContainer.GetConnectionString(),
                    ["EmailOptions:SmtpServer"] = "localhost",
                    ["EmailOptions:SmtpPort"] = "1025",
                    ["EmailOptions:SmtpUsername"] = "test",
                    ["EmailOptions:SmtpPassword"] = "test",
                    ["EmailOptions:FromEmail"] = "test@example.com",
                    ["EmailOptions:FromName"] = "Test",
                    ["JwtOptions:Secret"] = "test-secret-key-for-testing-at-least-32-characters-long",
                    ["JwtOptions:Issuer"] = "test-issuer",
                    ["JwtOptions:Audience"] = "test-audience",
                    ["JwtOptions:ExpirationMinutes"] = "60"
                }));

                builder.ConfigureServices(services =>
                {
                    services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));
                });

                builder.UseEnvironment("Testing");
            });

        HttpClient = _factory.CreateClient();

        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
        _scope.Dispose();
        HttpClient.Dispose();
        await _factory.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    // Metody pomocnicze dla test�w
    protected async Task<Guid> RegisterUserAsync(
        string? email = null,
        string password = "Password123!",
        string firstName = "Test",
        string lastName = "User")
    {
        // Generate unique email if not provided
        email ??= $"test-{Guid.NewGuid()}@example.com";

        var registerRequest = new
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = "+1234567890"
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);
        response.EnsureSuccessStatusCode();

        Guid userId = await response.Content.ReadFromJsonAsync<Guid>();
        return userId;
    }

    protected async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);
        response.EnsureSuccessStatusCode();

        LoginResponse? loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    protected async Task ExecuteInTransactionAsync(Func<ApplicationDbContext, Task> action)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await action(DbContext);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private record LoginResponse(string Token);
}
