using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagementSystemAPI.Models.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace OrderManagementSystemAPI.Test.API;
public class ReportsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ReportsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDailySummary_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/reports/daily-summary");
        var summary = await response.Content.ReadFromJsonAsync<DailySummaryDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(summary);
        Assert.True(summary.TotalOrders >= 0);
        Assert.True(summary.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetDailySummary_WithSpecificDate_ReturnsCorrectDate()
    {
        // Arrange
        var client = _factory.CreateClient();
        var targetDate = new DateTime(2025, 10, 26);

        // Act
        var response = await client.GetAsync($"/api/reports/daily-summary?date={targetDate:yyyy-MM-dd}");
        var summary = await response.Content.ReadFromJsonAsync<DailySummaryDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(summary);
        Assert.Equal(targetDate.Date, summary.Date.Date);
    }

    [Fact]
    public async Task GetLowStock_ReturnsProductsBelowThreshold()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/reports/low-stock?threshold=5");
        var lowStockProducts = await response.Content.ReadFromJsonAsync<List<LowStockProductDto>>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.NotNull(lowStockProducts);
        Assert.All(lowStockProducts, p => Assert.True(p.StockQuantity < 5));
    }

    [Fact]
    public async Task GetLowStock_WithNegativeThreshold_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/reports/low-stock?threshold=-1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}