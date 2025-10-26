using Microsoft.AspNetCore.Mvc;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Queries;

namespace OrderManagementSystemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    public ReportsController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }
    [HttpGet("daily-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date = null)
    {
        var query = new GetDailySummaryQuery(date);
        var result = await _queryDispatcher.DispatchAsync<GetDailySummaryQuery, GetDailySummaryResult>(query);

        return Ok(result.Summary);
    }
    [HttpGet("low-stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
    {
        if (threshold < 0)
            return BadRequest(new { message = "Threshold must be non-negative" });

        var query = new GetLowStockProductsQuery(threshold);
        var result = await _queryDispatcher.DispatchAsync<GetLowStockProductsQuery, GetLowStockProductsResult>(query);

        return Ok(result.Products);
    }
}