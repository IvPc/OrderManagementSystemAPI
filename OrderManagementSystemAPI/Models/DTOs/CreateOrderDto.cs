namespace OrderManagementSystemAPI.Models.DTOs;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
}