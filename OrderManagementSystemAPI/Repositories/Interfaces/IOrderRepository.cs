using OrderManagementSystemAPI.Models;

namespace OrderManagementSystemAPI.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<IEnumerable<Order>> GetOrdersByDateAsync(DateTime date);
    Task<Order?> GetByIdAsync(int id);
}