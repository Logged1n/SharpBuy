namespace Web.Api.IntegrationTests.Endpoints;

public class ProductEndpointsTests : BaseIntegrationTest
{

    [Fact]
    public async Task AddProduct_WithAuthentication_ShouldReturn201Created()
    {
        // Arrange
        string email = "product-test1@example.com";
        await RegisterUserAsync(email);
        string token = await GetAuthTokenAsync(email, "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create categories first
        CategoryResponse category = await CreateCategoryAsync(token);

        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Quantity = 10,
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { category.Id },
            MainPhotoPath = "/photos/test.jpg"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        Guid productId = await response.Content.ReadFromJsonAsync<Guid>();
        productId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task AddProduct_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Quantity = 10,
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { Guid.NewGuid() },
            MainPhotoPath = "/photos/test.jpg"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddProduct_WithInvalidCategoryIds_ShouldReturn400BadRequest()
    {
        // Arrange
        string email = "product-test2@example.com";
        await RegisterUserAsync(email);
        string token = await GetAuthTokenAsync(email, "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var productRequest = new
        {
            Name = "Test Product",
            Description = "Test Description",
            Quantity = 10,
            Price = new { Amount = 99.99m, Currency = "USD" },
            CategoryIds = new[] { Guid.NewGuid() }, // Non-existent category
            MainPhotoPath = "/photos/test.jpg"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Description", 10, 99.99)] // Empty name
    [InlineData("Product", "", 10, 99.99)]      // Empty description
    [InlineData("Product", "Description", 0, 99.99)] // Zero quantity
    [InlineData("Product", "Description", 10, 0)] // Zero price
    public async Task AddProduct_WithInvalidData_ShouldReturn400BadRequest(
        string name, string description, int quantity, decimal price)
    {
        // Arrange
        string email = $"product-test-{Guid.NewGuid()}@example.com";
        await RegisterUserAsync(email);
        string token = await GetAuthTokenAsync(email, "Password123!");
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        CategoryResponse category = await CreateCategoryAsync(token);

        var productRequest = new
        {
            Name = name,
            Description = description,
            Quantity = quantity,
            Price = new { Amount = price, Currency = "USD" },
            CategoryIds = new[] { category.Id },
            MainPhotoPath = "/photos/test.jpg"
        };

        // Act
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var categoryRequest = new { Name = "Electronics" };
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/categories", categoryRequest);
        response.EnsureSuccessStatusCode();

        Guid categoryId = await response.Content.ReadFromJsonAsync<Guid>();
        return new CategoryResponse(categoryId, "Electronics");
    }

    private record CategoryResponse(Guid Id, string Name);
}
