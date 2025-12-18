using Application.Analytics;
using Application.Analytics.GetSalesAnalytics;
using Application.Analytics.GetProductAnalytics;
using Application.Analytics.GetCustomerAnalytics;
using Application.Analytics.GetOrderAnalytics;
using Domain.Addresses;
using Domain.Categories;
using Domain.Orders;
using Domain.Products;
using Domain.Users;
using Shouldly;
using SharedKernel;
using SharedKernel.ValueObjects;
using Tests.Integration.Infrastructure;
using Xunit;

namespace Application.IntegrationTests.Analytics;

public class AnalyticsQueryHandlersTests : IntegrationTestBase
{
    [Fact]
    public async Task GetSalesAnalytics_WithNoOrders_ShouldReturnZeroMetrics()
    {
        // Arrange
        var handler = new GetSalesAnalyticsQueryHandler(DbContext);
        var query = new GetSalesAnalyticsQuery(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            Granularity.Day);

        // Act
        Result<SalesAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalRevenue.ShouldBe(0);
        result.Value.TotalOrders.ShouldBe(0);
        result.Value.AverageOrderValue.ShouldBe(0);
    }

    [Fact]
    public async Task GetSalesAnalytics_WithCompletedOrders_ShouldCalculateCorrectMetrics()
    {
        // Arrange
        var user = DomainUser.Create("test@test.com", "Test", "User", "555-1234");
        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();

        var category = Category.Create("Test Category");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        var product1 = Product.Create("Product 1", "Description 1", 10, 100m, "USD", "/photo1.jpg");
        product1.AddToCategory(category.Id);
        var product2 = Product.Create("Product 2", "Description 2", 10, 200m, "USD", "/photo2.jpg");
        product2.AddToCategory(category.Id);
        DbContext.Products.Add(product1);
        DbContext.Products.Add(product2);
        await DbContext.SaveChangesAsync();

        var address = Address.Create("123 Main St", null, "City", "12345", "Country");
        address.UserId = user.Id;
        DbContext.Addresses.Add(address);
        await DbContext.SaveChangesAsync();

        // Create completed orders
        var order1 = Order.Create(user.Id, address.Id);
        order1.AddOrderItem(product1.Id, "Product 1", new Money(100, "USD"), 1);
        order1.MoveToStatus(OrderStatus.Completed);

        var order2 = Order.Create(user.Id, address.Id);
        order2.AddOrderItem(product2.Id, "Product 2", new Money(200, "USD"), 2);
        order2.MoveToStatus(OrderStatus.Completed);

        DbContext.Orders.Add(order1);
        DbContext.Orders.Add(order2);
        await DbContext.SaveChangesAsync();

        var handler = new GetSalesAnalyticsQueryHandler(DbContext);
        var query = new GetSalesAnalyticsQuery(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            Granularity.Day);

        // Act
        Result<SalesAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalRevenue.ShouldBe(500m); // 100 + 400
        result.Value.TotalOrders.ShouldBe(2);
        result.Value.AverageOrderValue.ShouldBe(250m);
    }

    [Fact]
    public async Task GetProductAnalytics_WithNoOrders_ShouldReturnEmptyTopProducts()
    {
        // Arrange
        var handler = new GetProductAnalyticsQueryHandler(DbContext);
        var query = new GetProductAnalyticsQuery(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            Granularity.Day);

        // Act
        Result<ProductAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalProductsSold.ShouldBe(0);
        result.Value.TotalRevenue.ShouldBe(0);
        result.Value.TopProducts.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCustomerAnalytics_WithNoCustomers_ShouldReturnZeroMetrics()
    {
        // Arrange
        var handler = new GetCustomerAnalyticsQueryHandler(DbContext);
        var query = new GetCustomerAnalyticsQuery(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            Granularity.Day);

        // Act
        Result<CustomerAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCustomers.ShouldBe(0);
        result.Value.NewCustomers.ShouldBe(0);
        result.Value.ReturningCustomers.ShouldBe(0);
        result.Value.AverageCustomerValue.ShouldBe(0);
    }

    [Fact]
    public async Task GetOrderAnalytics_WithNoOrders_ShouldReturnZeroMetrics()
    {
        // Arrange
        var handler = new GetOrderAnalyticsQueryHandler(DbContext);
        var query = new GetOrderAnalyticsQuery(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            Granularity.Day);

        // Act
        Result<OrderAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalOrders.ShouldBe(0);
        result.Value.CompletedOrders.ShouldBe(0);
        result.Value.PendingOrders.ShouldBe(0);
        result.Value.CompletionRate.ShouldBe(0);
    }

    [Fact]
    public async Task GetOrderAnalytics_WithMixedOrderStatuses_ShouldCalculateCorrectly()
    {
        // Arrange
        var user = DomainUser.Create("test@test.com", "Test", "User", "555-1234");
        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();

        var category = Category.Create("Test Category");
        DbContext.Categories.Add(category);
        await DbContext.SaveChangesAsync();

        // Create 5 products
        var products = new List<Product>();
        for (int i = 0; i < 5; i++)
        {
            var product = Product.Create($"Product {i}", $"Description {i}", 10, 100m, "USD", $"/photo{i}.jpg");
            product.AddToCategory(category.Id);
            products.Add(product);
            DbContext.Products.Add(product);
        }
        await DbContext.SaveChangesAsync();

        var address = Address.Create("123 Main St", null, "City", "12345", "Country");
        address.UserId = user.Id;
        DbContext.Addresses.Add(address);
        await DbContext.SaveChangesAsync();

        // Create 3 completed orders
        for (int i = 0; i < 3; i++)
        {
            var completedOrder = Order.Create(user.Id, address.Id);
            completedOrder.AddOrderItem(products[i].Id, $"Product {i}", new Money(100, "USD"), 1);
            completedOrder.MoveToStatus(OrderStatus.Completed);
            DbContext.Orders.Add(completedOrder);
        }

        // Create 2 open orders
        for (int i = 0; i < 2; i++)
        {
            var openOrder = Order.Create(user.Id, address.Id);
            openOrder.AddOrderItem(products[i + 3].Id, $"Product {i + 3}", new Money(100, "USD"), 1);
            DbContext.Orders.Add(openOrder);
        }

        await DbContext.SaveChangesAsync();

        var handler = new GetOrderAnalyticsQueryHandler(DbContext);
        var query = new GetOrderAnalyticsQuery(
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1),
            Granularity.Day);

        // Act
        Result<OrderAnalyticsResponse> result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalOrders.ShouldBe(5);
        result.Value.CompletedOrders.ShouldBe(3);
        result.Value.PendingOrders.ShouldBe(2);
        result.Value.CompletionRate.ShouldBe(60.00m); // (3/5) * 100
    }
}
