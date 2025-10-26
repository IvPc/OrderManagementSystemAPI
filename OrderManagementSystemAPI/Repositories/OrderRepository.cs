using Microsoft.EntityFrameworkCore;
using OrderManagementSystemAPI.Data;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderManagementContext _context;

    public OrderRepository(OrderManagementContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<IEnumerable<Order>> GetOrdersByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderDate >= startOfDay && o.OrderDate < endOfDay)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}