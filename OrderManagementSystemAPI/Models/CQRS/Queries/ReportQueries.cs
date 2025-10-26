namespace OrderManagementSystemAPI.Models.CQRS.Queries;

public record GetDailySummaryQuery(DateTime? Date = null);
public record GetLowStockProductsQuery(int Threshold = 5);
