namespace Web.Api.IntegrationTests;

/// <summary>
/// Base class for API integration tests
/// </summary>
[Collection(nameof(WebApiCollection))]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly WebApiFactory Factory;
    protected HttpClient HttpClient = null!;

    protected BaseIntegrationTest(WebApiFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        HttpClient = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        HttpClient.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper method to get JWT token for authentication
    /// </summary>
    protected async Task<string> GetAuthTokenAsync(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);
        response.EnsureSuccessStatusCode();

        LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    /// <summary>
    /// Helper method to register a new user
    /// </summary>
    protected async Task<Guid> RegisterUserAsync(
        string email = "test@example.com",
        string password = "Password123!",
        string firstName = "John",
        string lastName = "Doe",
        string phoneNumber = "+1234567890")
    {
        var registerRequest = new
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber
        };

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);
        response.EnsureSuccessStatusCode();

        string? locationHeader = response.Headers.Location?.ToString();
#pragma warning disable S6608 // Prefer indexing instead of "Enumerable" methods on types implementing "IList"
        string? userId = locationHeader?.Split('/').Last();
#pragma warning restore S6608 // Prefer indexing instead of "Enumerable" methods on types implementing "IList"
        return Guid.Parse(userId!);
    }

    private record LoginResponse(string Token);
}

/// <summary>
/// Collection definition for sharing WebApiFactory across test classes
/// </summary>
[CollectionDefinition(nameof(WebApiCollection))]
public class WebApiCollection : ICollectionFixture<WebApiFactory>
{
}
