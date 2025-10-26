using Microsoft.Extensions.Caching.Memory;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Models.DTOs;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Queries;

public class GetDailySummaryQueryHandler : IQueryHandler<GetDailySummaryQuery, GetDailySummaryResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMemoryCache _cache;
    private readonly TimeProvider _timeProvider;

    public GetDailySummaryQueryHandler(
        IOrderRepository orderRepository,
        IMemoryCache cache,
        TimeProvider timeProvider)
    {
        _orderRepository = orderRepository;
        _cache = cache;
        _timeProvider = timeProvider;
    }

    public async Task<GetDailySummaryResult> HandleAsync(GetDailySummaryQuery query)
    {
        var targetDate = (query.Date ?? _timeProvider.GetUtcNow().UtcDateTime).Date;
        var cacheKey = $"{Constants.CacheKeyPrefix}{targetDate:yyyy-MM-dd}";

        if (_cache.TryGetValue(cacheKey, out DailySummaryDto? cachedSummary) && cachedSummary != null)
        {
            return new GetDailySummaryResult(cachedSummary);
        }

        var orders = await _orderRepository.GetOrdersByDateAsync(targetDate);
        var summary = new DailySummaryDto
        {
            Date = targetDate,
            TotalOrders = orders.Count(),
            TotalRevenue = orders.Sum(o => o.TotalAmount)
        };

        // Cache for 5 minutes
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(cacheKey, summary, cacheOptions);

        return new GetDailySummaryResult(summary);
    }
}