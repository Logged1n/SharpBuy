namespace Web.Api.IntegrationTests.Endpoints;

public class UserEndpointsTests : BaseIntegrationTest
{
    [Fact]
    public async Task RegisterUser_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "987654321"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        Guid userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldReturn409Conflict()
    {
        // Arrange
        string email = "duplicate@example.com";
        await RegisterUserAsync(email);

        var registerRequest = new
        {
            Email = email,
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "123456789"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidEmail_ShouldReturn400BadRequest()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "invalid-email",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "123456789"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        // Arrange
        string email = "logintest@example.com";
        string password = "Password123!";
        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        LoginResponse? loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.ShouldNotBeNull();
        loginResponse!.Token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401Unauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await HttpClient.GetAsync($"/users/{userId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturn404NotFound()
    {
        // Arrange
        await RegisterUserAsync("authuser@example.com");
        string token = await GetAuthTokenAsync("authuser@example.com", "Password123!");

        HttpClient client = HttpClient;
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var nonExistentUserId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/users/{nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private record LoginResponse(string Token);
    private record UserResponse(Guid Id, string Email, string FirstName, string LastName);
}
