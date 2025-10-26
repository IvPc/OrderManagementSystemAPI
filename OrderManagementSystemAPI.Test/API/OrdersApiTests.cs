using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagementSystemAPI.Models.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace OrderManagementSystemAPI.Test.API;

public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedWithSummary()
    {
        // Arrange
        var client = _factory.CreateClient();
        var orderDto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", orderDto);
        var orderSummary = await response.Content.ReadFromJsonAsync<OrderSummaryDto>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(orderSummary);
        Assert.True(orderSummary.TotalAmount > 0);
        Assert.Single(orderSummary.Items);
    }

    [Fact]
    public async Task CreateOrder_WithInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var orderDto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 3, Quantity = 100 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", orderDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        var orderDto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 99999, Quantity = 1 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", orderDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}