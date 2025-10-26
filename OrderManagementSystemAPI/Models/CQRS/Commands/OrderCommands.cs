using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Models.CQRS.Commands;

public record CreateOrderCommand(List<OrderItemDto> Items);