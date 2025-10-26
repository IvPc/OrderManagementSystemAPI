using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Models.CQRS.Commands;

public record CreateProductResult(Product Product);
public record UpdateProductResult(Product? Product);
public record DeleteProductResult(bool Success);
public record CreateOrderResult(OrderSummaryDto OrderSummary);