using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderManagementSystemAPI.CQRS.Queries;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories;

namespace OrderManagementSystemAPI.Test.CQRS.Queries;

public class ReportQueryHandlerTests
{
    private OrderManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderManagementContext(options);
    }

    [Fact]
    public async Task GetDailySummaryQueryHandler_ReturnsCorrectSummary()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetDailySummaryQueryHandler(orderRepo, cache, TimeProvider.System);

        var targetDate = new DateTime(2025, 10, 26);
        var order1 = new Order { OrderDate = targetDate.AddHours(10), TotalAmount = 100m };
        var order2 = new Order { OrderDate = targetDate.AddHours(14), TotalAmount = 200m };
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var query = new GetDailySummaryQuery(targetDate);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(targetDate.Date, result.Summary.Date);
        Assert.Equal(2, result.Summary.TotalOrders);
        Assert.Equal(300m, result.Summary.TotalRevenue);
    }

    [Fact]
    public async Task GetLowStockProductsQueryHandler_ReturnsOnlyLowStock()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetLowStockProductsQueryHandler(repository);

        var product1 = new Product { Name = "Low", Price = 10m, StockQuantity = 2 };
        var product2 = new Product { Name = "High", Price = 20m, StockQuantity = 10 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var query = new GetLowStockProductsQuery(5);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Single(result.Products);
        Assert.Equal("Low", result.Products.First().Name);
    }
    [Fact]
    public async Task GetDailySummary_WithNoOrders_ReturnsZeros()
    {
        // Arrange
        var context = CreateContext();
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetDailySummaryQueryHandler(orderRepo, cache, TimeProvider.System);

        var query = new GetDailySummaryQuery(new DateTime(2025, 10, 26));

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(0, result.Summary.TotalOrders);
        Assert.Equal(0m, result.Summary.TotalRevenue);
    }

    [Fact]
    public async Task GetDailySummary_AtMidnight_ReturnsCorrectDate()
    {
        // Arrange
        var context = CreateContext();
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetDailySummaryQueryHandler(orderRepo, cache, TimeProvider.System);

        var targetDate = new DateTime(2025, 10, 26, 0, 0, 0);
        var order = new Order { OrderDate = targetDate, TotalAmount = 100m };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var query = new GetDailySummaryQuery(targetDate);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.Summary.TotalOrders);
        Assert.Equal(targetDate.Date, result.Summary.Date);
    }

    [Fact]
    public async Task GetDailySummary_AtEndOfDay_ExcludesNextDay()
    {
        // Arrange
        var context = CreateContext();
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetDailySummaryQueryHandler(orderRepo, cache, TimeProvider.System);

        var targetDate = new DateTime(2025, 10, 26);
        var order1 = new Order { OrderDate = targetDate.AddHours(23).AddMinutes(59), TotalAmount = 100m };
        var order2 = new Order { OrderDate = targetDate.AddDays(1), TotalAmount = 200m }; // Next day
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var query = new GetDailySummaryQuery(targetDate);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(1, result.Summary.TotalOrders);
        Assert.Equal(100m, result.Summary.TotalRevenue);
    }

    [Fact]
    public async Task GetDailySummary_WithMultipleCalls_UsesCacheOnSecondCall()
    {
        // Arrange
        var context = CreateContext();
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetDailySummaryQueryHandler(orderRepo, cache, TimeProvider.System);

        var targetDate = new DateTime(2025, 10, 26);
        var order = new Order { OrderDate = targetDate, TotalAmount = 100m };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var query = new GetDailySummaryQuery(targetDate);
        var result1 = await handler.HandleAsync(query);

        var order2 = new Order { OrderDate = targetDate, TotalAmount = 200m };
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        var result2 = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(result1.Summary.TotalOrders, result2.Summary.TotalOrders);
        Assert.Equal(100m, result2.Summary.TotalRevenue);
    }

    [Fact]
    public async Task GetLowStockProducts_WithThresholdZero_ReturnsOnlyZeroStock()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetLowStockProductsQueryHandler(repository);

        var product1 = new Product { Name = "Zero Stock", Price = 10m, StockQuantity = 0 };
        var product2 = new Product { Name = "One Stock", Price = 20m, StockQuantity = 1 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var query = new GetLowStockProductsQuery(0);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task GetLowStockProducts_WithVeryHighThreshold_ReturnsAllProducts()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetLowStockProductsQueryHandler(repository);

        var product1 = new Product { Name = "Product1", Price = 10m, StockQuantity = 100 };
        var product2 = new Product { Name = "Product2", Price = 20m, StockQuantity = 200 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var query = new GetLowStockProductsQuery(1000);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Products.Count());
    }

    [Fact]
    public async Task GetLowStockProducts_WithNoProducts_ReturnsEmpty()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetLowStockProductsQueryHandler(repository);
        var query = new GetLowStockProductsQuery(5);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task GetLowStockProducts_WithMixedStockLevels_ReturnsCorrectSubset()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetLowStockProductsQueryHandler(repository);

        var products = new List<Product>
        {
            new() { Name = "Stock0", Price = 10m, StockQuantity = 0 },
            new() { Name = "Stock1", Price = 10m, StockQuantity = 1 },
            new() { Name = "Stock4", Price = 10m, StockQuantity = 4 },
            new() { Name = "Stock5", Price = 10m, StockQuantity = 5 },
            new() { Name = "Stock10", Price = 10m, StockQuantity = 10 }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var query = new GetLowStockProductsQuery(5);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(3, result.Products.Count());
        Assert.All(result.Products, p => Assert.True(p.StockQuantity < 5));
    }
}