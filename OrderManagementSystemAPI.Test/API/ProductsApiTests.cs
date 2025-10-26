using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagementSystemAPI.Models.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace OrderManagementSystemAPI.Test.API;

public class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessAndProducts()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productDto = new CreateProductDto
        {
            Name = "Integration Test Product",
            Price = 199.99m,
            StockQuantity = 50
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/products", productDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPrice_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var productDto = new CreateProductDto
        {
            Name = "Invalid Product",
            Price = -10m,
            StockQuantity = 10
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/products", productDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_ThenGet_ReturnsUpdatedData()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateProductDto
        {
            Name = "Original Name",
            Price = 100m,
            StockQuantity = 10
        };
        var createResponse = await client.PostAsJsonAsync("/api/products", createDto);
        var createdProduct = await createResponse.Content.ReadFromJsonAsync<Models.Product>();

        var updateDto = new UpdateProductDto
        {
            Name = "Updated Name",
            Price = 150m,
            StockQuantity = 20
        };

        // Act
        var updateResponse = await client.PutAsJsonAsync($"/api/products/{createdProduct!.Id}", updateDto);
        var getResponse = await client.GetAsync($"/api/products/{createdProduct.Id}");
        var updatedProduct = await getResponse.Content.ReadFromJsonAsync<Models.Product>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Name", updatedProduct.Name);
        Assert.Equal(150m, updatedProduct.Price);
        Assert.Equal(20, updatedProduct.StockQuantity);
    }
}