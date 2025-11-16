namespace Web.Api.IntegrationTests.Endpoints;

public class ProductEndpointsTests : BaseIntegrationTest
{
    public ProductEndpointsTests(WebApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddProduct_WithAuthentication_ShouldReturn201Created()
    {
        // Arrange
        await RegisterUserAsync();
        var token = await GetAuthTokenAsync("test@example.com", "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create categories first
        var category = await CreateCategoryAsync(token);

        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { category.Id }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var productId = await response.Content.ReadFromJsonAsync<Guid>();
        productId.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AddProduct_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { Guid.NewGuid() }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddProduct_WithInvalidCategoryIds_ShouldReturn400BadRequest()
    {
        // Arrange
        await RegisterUserAsync();
        var token = await GetAuthTokenAsync("test@example.com", "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { Guid.NewGuid() } // Non-existent category
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Description", 99.99)] // Empty name
    [InlineData("Product", "", 99.99)]      // Empty description
    [InlineData("Product", "Description", 0)] // Zero price
    public async Task AddProduct_WithInvalidData_ShouldReturn400BadRequest(
        string name, string description, decimal price)
    {
        // Arrange
        await RegisterUserAsync();
        var token = await GetAuthTokenAsync("test@example.com", "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var category = await CreateCategoryAsync(token);

        var productRequest = new
        {
            Name = name,
            Description = description,
            Price = new { Amount = price, Currency = "USD" },
            CategoryIds = new[] { category.Id }
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var categoryRequest = new { Name = "Electronics" };
        var response = await HttpClient.PostAsJsonAsync("/categories", categoryRequest);
        response.EnsureSuccessStatusCode();

        var categoryId = await response.Content.ReadFromJsonAsync<Guid>();
        return new CategoryResponse(categoryId, "Electronics");
    }

    private record CategoryResponse(Guid Id, string Name);
}
