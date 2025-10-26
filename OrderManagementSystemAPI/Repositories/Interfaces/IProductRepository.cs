using OrderManagementSystemAPI.Models;

namespace OrderManagementSystemAPI.Repositories.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task SoftDeleteAsync(int id);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
}