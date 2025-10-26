namespace OrderManagementSystemAPI.Models.DTOs;
public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}
