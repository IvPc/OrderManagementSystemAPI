using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Models.CQRS.Queries;

public record GetAllProductsResult(IEnumerable<Product> Products);
public record GetProductByIdResult(Product? Product);
public record GetOrderByIdResult(Order? Order);
public record GetDailySummaryResult(DailySummaryDto Summary);
public record GetLowStockProductsResult(IEnumerable<LowStockProductDto> Products);