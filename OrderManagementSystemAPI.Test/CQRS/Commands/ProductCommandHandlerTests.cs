using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.CQRS.Commands;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Repositories;

namespace OrderManagementSystemAPI.Test.CQRS.Commands;

public class ProductCommandHandlerTests
{
    private OrderManagementContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrderManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OrderManagementContext(options);
    }

    [Fact]
    public async Task CreateProductCommandHandler_CreatesProduct()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 99.99m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal("Test Product", result.Product.Name);
        Assert.Equal(99.99m, result.Product.Price);
        Assert.Equal(10, result.Product.StockQuantity);
    }

    [Fact]
    public async Task UpdateProductCommandHandler_UpdatesExistingProduct()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new UpdateProductCommandHandler(repository);

        var product = new Product { Name = "Old Name", Price = 50m, StockQuantity = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new UpdateProductCommand(product.Id, "New Name", 75m, 15);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal("New Name", result.Product.Name);
        Assert.Equal(75m, result.Product.Price);
        Assert.Equal(15, result.Product.StockQuantity);
    }

    [Fact]
    public async Task UpdateProductCommandHandler_ReturnsNull_WhenProductNotFound()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new UpdateProductCommandHandler(repository);
        var command = new UpdateProductCommand(999, "Name", 100m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Product);
    }

    [Fact]
    public async Task DeleteProductCommandHandler_DeletesProduct()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new DeleteProductCommandHandler(repository);

        var product = new Product { Name = "To Delete", Price = 10m, StockQuantity = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new DeleteProductCommand(product.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.Success);
        var deletedProduct = await context.Products.FindAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task SoftDeleteProductCommandHandler_MarkAsDeleted()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new SoftDeleteProductCommandHandler(repository);

        var product = new Product { Name = "To Soft Delete", Price = 10m, StockQuantity = 5 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new SoftDeleteProductCommand(product.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.Success);
        var softDeletedProduct = await context.Products.FindAsync(product.Id);
        Assert.NotNull(softDeletedProduct);
        Assert.True(softDeletedProduct.IsDeleted);
    }
    [Fact]
    public async Task CreateProduct_WithZeroStock_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Zero Stock Product", 99.99m, 0);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(0, result.Product.StockQuantity);
    }

    [Fact]
    public async Task CreateProduct_WithVeryLargePriceAndStock_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Expensive Item", 999999999.99m, int.MaxValue);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(999999999.99m, result.Product.Price);
        Assert.Equal(int.MaxValue, result.Product.StockQuantity);
    }

    [Fact]
    public async Task CreateProduct_WithMinimalPrice_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Cheap Item", 0.01m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(0.01m, result.Product.Price);
    }

    [Fact]
    public async Task CreateProduct_WithVeryLongName_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new CreateProductCommandHandler(repository);
        var longName = new string('A', 200); // Max length
        var command = new CreateProductCommand(longName, 99.99m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(200, result.Product.Name.Length);
    }

    [Fact]
    public async Task UpdateProduct_WithSameValues_Succeeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new UpdateProductCommandHandler(repository);

        var product = new Product { Name = "Test", Price = 100m, StockQuantity = 10 };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new UpdateProductCommand(product.Id, "Test", 100m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal("Test", result.Product.Name);
    }

    [Fact]
    public async Task UpdateProduct_WithNegativeId_ReturnsNull()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new UpdateProductCommandHandler(repository);
        var command = new UpdateProductCommand(-1, "Test", 100m, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Product);
    }

    [Fact]
    public async Task SoftDelete_AlreadyDeletedProduct_StillSucceeds()
    {
        // Arrange
        var context = CreateContext();
        var repository = new ProductRepository(context, TimeProvider.System);
        var handler = new SoftDeleteProductCommandHandler(repository);

        var product = new Product
        {
            Name = "Test",
            Price = 10m,
            StockQuantity = 5,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new SoftDeleteProductCommand(product.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result.Success);
    }
}