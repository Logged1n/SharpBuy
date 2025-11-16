namespace Web.Api.IntegrationTests.Endpoints;

public class UserEndpointsTests : BaseIntegrationTest
{
    public UserEndpointsTests(WebApiFactory factory) : base(factory)
    {
    }

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
            PhoneNumber = "+1987654321"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterUser_WithDuplicateEmail_ShouldReturn409Conflict()
    {
        // Arrange
        var email = "duplicate@example.com";
        await RegisterUserAsync(email);

        var registerRequest = new
        {
            Email = email,
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
            PhoneNumber = "+1234567890"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/users/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithToken()
    {
        // Arrange
        var email = "logintest@example.com";
        var password = "Password123!";
        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
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
        var response = await HttpClient.PostAsJsonAsync("/users/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturn200WithUser()
    {
        // Arrange
        var userId = await RegisterUserAsync("getuser@example.com");
        var token = await GetAuthTokenAsync("getuser@example.com", "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.GetAsync($"/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Email.Should().Be("GETUSER@EXAMPLE.COM"); // Email is uppercase
    }

    [Fact]
    public async Task GetUserById_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturn404NotFound()
    {
        // Arrange
        await RegisterUserAsync("authuser@example.com");
        var token = await GetAuthTokenAsync("authuser@example.com", "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var nonExistentUserId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"/users/{nonExistentUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private record LoginResponse(string Token);
    private record UserResponse(Guid Id, string Email, string FirstName, string LastName);
}
