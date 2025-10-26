using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderManagementContext _context;
    private readonly TimeProvider _timeProvider;

    public ProductRepository(OrderManagementContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null && !product.IsDeleted)
        {
            product.IsDeleted = true;
            product.DeletedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.StockQuantity < threshold)
            .ToListAsync();
    }
}