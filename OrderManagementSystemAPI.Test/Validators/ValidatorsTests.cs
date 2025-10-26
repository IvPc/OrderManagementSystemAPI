using OrderManagementSystemAPI.Models.DTOs;
using OrderManagementSystemAPI.Validators;

namespace OrderManagementSystemAPI.Test.Validators;


public class ValidatorsTests
{
    [Fact]
    public async Task CreateProduct_WithEmptyName_FailsValidation()
    {
        var dto = new CreateProductDto { Name = "", Price = 10m, StockQuantity = 5 };

        var validator = new CreateProductDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task CreateProduct_WithNegativePrice_FailsValidation()
    {
        var dto = new CreateProductDto { Name = "Test", Price = -10m, StockQuantity = 5 };

        var validator = new CreateProductDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task CreateProduct_WithNegativeStock_FailsValidation()
    {
        var dto = new CreateProductDto { Name = "Test", Price = 10m, StockQuantity = -5 };

        var validator = new CreateProductDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StockQuantity");
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_FailsValidation()
    {
        var dto = new CreateOrderDto { Items = new List<OrderItemDto>() };

        var validator = new CreateOrderDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task CreateOrder_WithZeroQuantity_FailsValidation()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 0 }
            }
        };

        var validator = new CreateOrderDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task CreateOrder_WithNegativeQuantity_FailsValidation()
    {
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = -5 }
            }
        };

        var validator = new CreateOrderDtoValidator();
        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
    }
}