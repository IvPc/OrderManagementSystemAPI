using Microsoft.Extensions.Caching.Memory;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Models.DTOs;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Commands;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IMemoryCache _cache;


    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IMemoryCache cache,
        TimeProvider timeProvider)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cache = cache;
        _timeProvider = timeProvider;
    }

    public async Task<CreateOrderResult> HandleAsync(CreateOrderCommand command)
    {
        decimal totalAmount = 0;
        var orderItems = new List<KeyValuePair<int, OrderItem>>();
        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId)
                                ?? throw new KeyNotFoundException($"Product with ID {item.ProductId} not found");

            var orderQuantity = orderItems.Where(o => o.Key == product.Id).Select(o => o.Value.Quantity).Sum();

            if (product.StockQuantity < item.Quantity + orderQuantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity + orderQuantity}");

            var subtotal = product.Price * item.Quantity;
            totalAmount += subtotal;
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Subtotal = subtotal
            };
            orderItems.Add(new(product.Id, orderItem));
        }
        // Deduct stock
        foreach (var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }

        var orders = orderItems.Select(o => o.Value);
        // Create order
        var order = new Order
        {
            OrderDate = _timeProvider.GetUtcNow().UtcDateTime,
            TotalAmount = totalAmount,
            OrderItems = orders.ToList()
        };

        await _orderRepository.AddAsync(order);
        var summary = new OrderSummaryDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Items = [.. orders.Select(oi => new OrderItemSummaryDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal
            })]
        };


        var cacheKey = $"{Constants.CacheKeyPrefix}{_timeProvider.GetUtcNow().UtcDateTime.Date:yyyy-MM-dd}";
        _cache.Remove(cacheKey);

        return new CreateOrderResult(summary);
    }
}