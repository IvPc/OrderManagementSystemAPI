namespace OrderManagementSystemAPI.Models.CQRS.Commands;

public record CreateProductCommand(string Name, decimal Price, int StockQuantity);
public record UpdateProductCommand(int Id, string Name, decimal Price, int StockQuantity);
public record DeleteProductCommand(int Id);
public record SoftDeleteProductCommand(int Id);