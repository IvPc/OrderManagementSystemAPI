using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OrderManagementSystemAPI.CQRS.Commands;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Models.DTOs;
using OrderManagementSystemAPI.Repositories;

namespace OrderManagementSystemAPI.Test.CQRS.Commands;

public class OrderCommandHandlerTests
{
    private OrderManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderManagementContext(options);
    }

    [Fact]
    public async Task CreateOrderCommandHandler_CreatesOrder_WithSufficientStock()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Test Product", Price = 100m, StockQuantity = 10 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 3 } };
        var command = new CreateOrderCommand(items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.OrderSummary);
        Assert.Equal(300m, result.OrderSummary.TotalAmount);

        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(7, updatedProduct!.StockQuantity);
    }

    [Fact]
    public async Task CreateOrderCommandHandler_ThrowsException_WithInsufficientStock()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Low Stock", Price = 100m, StockQuantity = 2 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 5 } };
        var command = new CreateOrderCommand(items);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateOrderCommandHandler_ThrowsException_WithNonExistentProduct()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var items = new List<OrderItemDto> { new() { ProductId = 999, Quantity = 1 } };
        var command = new CreateOrderCommand(items);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.HandleAsync(command));
    }
    [Fact]
    public async Task CreateOrder_WithExactAvailableStock_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Test", Price = 100m, StockQuantity = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 5 } };
        var command = new CreateOrderCommand(items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(500m, result.OrderSummary.TotalAmount);
        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(0, updatedProduct!.StockQuantity);
    }

    [Fact]
    public async Task CreateOrder_WithOneItemOverStock_ThrowsException()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Test", Price = 100m, StockQuantity = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 6 } };
        var command = new CreateOrderCommand(items);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));

        Assert.Contains("Insufficient stock", exception.Message);

        var unchangedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(5, unchangedProduct!.StockQuantity);
    }

    [Fact]
    public async Task CreateOrder_WithMultipleItems_OneInsufficientStock_ThrowsException()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product1 = new Product { Name = "Product1", Price = 100m, StockQuantity = 10 };
        var product2 = new Product { Name = "Product2", Price = 50m, StockQuantity = 2 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto>
        {
            new() { ProductId = product1.Id, Quantity = 5 },
            new() { ProductId = product2.Id, Quantity = 5 }  
        };
        var command = new CreateOrderCommand(items);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));

        // Verify NO stock was deducted from either product (all or nothing)
        var unchangedProduct1 = await context.Products.FindAsync(product1.Id);
        var unchangedProduct2 = await context.Products.FindAsync(product2.Id);
        Assert.Equal(10, unchangedProduct1!.StockQuantity);
        Assert.Equal(2, unchangedProduct2!.StockQuantity);
    }

    [Fact]
    public async Task CreateOrder_WithSoftDeletedProduct_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product
        {
            Name = "Deleted",
            Price = 100m,
            StockQuantity = 10,
            IsDeleted = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 1 } };
        var command = new CreateOrderCommand(items);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateOrder_WithVeryLargeQuantity_CalculatesCorrectly()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Bulk Item", Price = 0.50m, StockQuantity = 1000000 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto> { new() { ProductId = product.Id, Quantity = 100000 } };
        var command = new CreateOrderCommand(items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(50000m, result.OrderSummary.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_WithMixedPriceDecimalPrecision_CalculatesCorrectly()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product1 = new Product { Name = "Item1", Price = 10.99m, StockQuantity = 100 };
        var product2 = new Product { Name = "Item2", Price = 5.50m, StockQuantity = 100 };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto>
        {
            new() { ProductId = product1.Id, Quantity = 3 },
            new() { ProductId = product2.Id, Quantity = 2 }
        };
        var command = new CreateOrderCommand(items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(43.97m, result.OrderSummary.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_WithDuplicateProductIds_ProcessesCorrectly()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);

        var product = new Product { Name = "Product", Price = 10m, StockQuantity = 20 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var items = new List<OrderItemDto>
        {
            new() { ProductId = product.Id, Quantity = 5 },
            new() { ProductId = product.Id, Quantity = 3 }
        };
        var command = new CreateOrderCommand(items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(80m, result.OrderSummary.TotalAmount);
        Assert.Equal(2, result.OrderSummary.Items.Count);

        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(12, updatedProduct!.StockQuantity); 
    }
    [Fact]
    public async Task CreateOrder_RaceCondition_BothOrdersProcessedSequentially()
    {
        // Arrange
        var context = CreateContext();
        var productRepo = new ProductRepository(context, TimeProvider.System);
        var orderRepo = new OrderRepository(context);

        var product = new Product { Name = "Popular Item", Price = 100m, StockQuantity = 10 };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler1 = new CreateOrderCommandHandler(orderRepo, productRepo, cache, TimeProvider.System);
        var handler2 = new CreateOrderCommandHandler(orderRepo, productRepo, cache,  TimeProvider.System);

        var command1 = new CreateOrderCommand(new List<OrderItemDto>
        {
            new() { ProductId = product.Id, Quantity = 6 }
        });

        var command2 = new CreateOrderCommand(new List<OrderItemDto>
        {
            new() { ProductId = product.Id, Quantity = 5 }
        });

        // Act
        var result1 = await handler1.HandleAsync(command1);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler2.HandleAsync(command2));

        // Assert
        Assert.Contains("Insufficient stock", exception.Message);
        var finalProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(4, finalProduct!.StockQuantity);
    }
}
