using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.CQRS.Queries;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories;

namespace OrderManagementSystemAPI.Test.CQRS.Queries;

public class ProductQueryHandlerTests
{
    private OrderManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderManagementContext(options);
    }

    [Fact]
    public async Task GetAllProductsQueryHandler_ReturnsAllNonDeletedProducts()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetAllProductsQueryHandler(repository);

        var product1 = new Product { Name = "Active", Price = 10m, StockQuantity = 5 };
        var product2 = new Product { Name = "Deleted", Price = 20m, StockQuantity = 3, IsDeleted = true };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var query = new GetAllProductsQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Single(result.Products);
        Assert.Equal("Active", result.Products.First().Name);
    }

    [Fact]
    public async Task GetProductByIdQueryHandler_ReturnsProduct_WhenExists()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetProductByIdQueryHandler(repository);

        var product = new Product { Name = "Test", Price = 100m, StockQuantity = 10 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var query = new GetProductByIdQuery(product.Id);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal("Test", result.Product.Name);
    }

    [Fact]
    public async Task GetProductByIdQueryHandler_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(999);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Null(result.Product);
    }

    [Fact]
    public async Task GetAllProducts_WithOnlySoftDeletedProducts_ReturnsEmpty()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetAllProductsQueryHandler(repository);

        var product1 = new Product { Name = "Deleted1", Price = 10m, StockQuantity = 5, IsDeleted = true };
        var product2 = new Product { Name = "Deleted2", Price = 20m, StockQuantity = 3, IsDeleted = true };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var query = new GetAllProductsQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task GetAllProducts_WithNoProducts_ReturnsEmpty()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetAllProductsQueryHandler(repository);
        var query = new GetAllProductsQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task GetProductById_WithZeroId_ReturnsNull()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new GetProductByIdQueryHandler(repository);
        var query = new GetProductByIdQuery(0);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Null(result.Product);
    }
}